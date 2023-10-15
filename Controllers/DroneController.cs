using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;

[ApiController]
[Route("api/[controller]")]
public class DroneController : ControllerBase
{
    private readonly DroneDbContext _context;

    public DroneController(DroneDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableDrones()
    {
        var availableDrones = await _context.Drones
            .Where(d => d.State != DroneState.LOADING)
            .Include(s => s.Medications)
            .ToListAsync();

        var result = availableDrones.Select(d => new DroneDTO
        {
            SerialNumber = d.SerialNumber,
            Model = d.Model.ToString(),
            State = d.State.ToString(),
            WeightLimit = d.WeightLimit,
            BatteryCapacity = d.BatteryCapacity,
            BatteryLevel = d.BatteryLevel,
            Medications = d.Medications.Select(s => new MedicationDTO
            {
                Code = s.Code,
                Name = s.Name,
                Weight = s.Weight,
                Image = s.Image
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterDrone(DroneDTO drone)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if(await _context.Drones.AnyAsync(d => d.SerialNumber == drone.SerialNumber))
        {
            ModelState.AddModelError("SerialNumber", "The serial number is already in use.");
            return BadRequest(ModelState);
        }

        try
        {
            Drone newDrone = new Drone
            {
                SerialNumber = drone.SerialNumber,
                Model = Enum.Parse<DroneModel>(drone.Model),
                State = Enum.Parse<DroneState>(drone.State),
                WeightLimit = drone.WeightLimit,
                BatteryCapacity = drone.BatteryCapacity,
                BatteryLevel = drone.BatteryLevel
            };

            _context.Drones.Add(newDrone);

            foreach(var med in drone.Medications)
            {
                Medication medication = new Medication
                {
                    Name = med.Name,
                    Code = med.Code,
                    Weight = med.Weight,
                    Image = med.Image,
                    DroneSerialNumber = newDrone.SerialNumber
                };

                _context.Medications.Add(medication);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(RegisterDrone), new { serialNumber = newDrone.SerialNumber });
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Error saving changes to the database: {ex.Message}");
        }
    }

    [HttpPost("{droneId}/load")]
    public async Task<IActionResult> LoadMedication(List<MedicationDTO> medications)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var droneId = HttpContext.Request.RouteValues["droneId"] as string;
        if(string.IsNullOrEmpty(droneId))
        {
            return BadRequest();
        }

        var drone = _context.Drones.Include(s => s.Medications).SingleOrDefault(e => e.SerialNumber == droneId);
        if(drone == null)
        {
            return NotFound();
        }

        if(drone.WeightLimit < (drone.Medications.Count > 0 ? drone.Medications.Sum(s => s.Weight) : 0) + medications.Sum(s => s.Weight))
        {
            return BadRequest("The medications weight exceeds the drone's weight limit.");
        }

        if(drone.State == DroneState.LOADING && drone.BatteryLevel < 25)
        {
            return BadRequest("The drone's battery level is below 25%. It cannot be in LOADING state.");
        }
        try
        {
            foreach(var medication in medications)
            {
                Medication med = new Medication();
                med.Name = medication.Name;
                med.Code = medication.Code;
                med.Weight = medication.Weight;
                med.Image = medication.Image;

                drone.Medications.Add(med);
            }
            await _context.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Error saving changes to the database: {ex.Message}");
        }
        return Ok();
    }

    [HttpGet("{droneId}/medications")]
    public async Task<IActionResult> GetLoadedMedications(string droneId)
    {
        var drone = await _context.Drones
            .Include(s => s.Medications)
            .SingleOrDefaultAsync(s => s.SerialNumber == droneId);

        if(drone == null)
        {
            return NotFound();
        }

        var result = drone.Medications.Select(s => new MedicationDTO
        {
            Code = s.Code,
            Name = s.Name,
            Weight = s.Weight,
            Image = s.Image
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{droneId}")]
    public async Task<IActionResult> GetBatteryLevel(string droneId)
    {
        var drone = await _context.Drones.FindAsync(droneId);
        if(drone == null)
        {
            return NotFound();
        }

        return Ok(drone.BatteryLevel);
    }

    [AutomaticRetry(Attempts = 3)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public void CheckBatteryLevels()
    {
        _context.Drones
            .ToList()
            .ForEach(drone =>
            {
                int batteryLevel = RecalculateBatteryLevel(drone);

                var eventLog = new EventLog
                {
                    DroneSerial = drone.SerialNumber,
                    Timestamp = DateTime.Now,
                    BatteryLevel = batteryLevel
                };

                _context.EventLogs.Add(eventLog);
            });

        _context.SaveChangesAsync();
    }

    internal int RecalculateBatteryLevel(Drone drone)
    {
        var random = new Random();
        var rangeBatteryLevel = random.Next(-drone.BatteryLevel, drone.BatteryCapacity + 1 - drone.BatteryLevel);

        drone.BatteryLevel = drone.BatteryLevel + rangeBatteryLevel;
        _context.SaveChangesAsync();

        return drone.BatteryLevel;
    }
}
