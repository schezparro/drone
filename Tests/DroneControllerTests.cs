using Xunit;
using Microsoft.EntityFrameworkCore;

public class RecurringTaskTests
{
    private DroneDbContext _context;
    private DroneController _controller;

    public RecurringTaskTests()
    {
        var options = new DbContextOptionsBuilder<DroneDbContext>()
          .UseInMemoryDatabase(databaseName: "TestDatabase")
          .Options;

        _context = new DroneDbContext(options);
        _controller = new DroneController(_context);
    }

    [Fact]
    public void CheckBatteryLevels_AddsEventLogForEachDrone()
    {
        AddTestDrones();
        for (int i = 0; i < 5; i++)
        {
            _controller.CheckBatteryLevels();
            Task.Delay(1000);
        }

        var eventLogs = _context.EventLogs.ToList();

        var filePath = "events.txt";
        var lines = eventLogs.Select(e =>
            {
                return $"{e.DroneSerial} - {e.BatteryLevel}";
            });
        File.WriteAllLines(filePath, lines);

        Assert.Equal(10, eventLogs.Count);
    }

    private void AddTestDrones()
    {
        _context.Drones.AddRange(
          new Drone { SerialNumber = "drone1", BatteryLevel = 85, BatteryCapacity = 100 },
          new Drone { SerialNumber = "drone2", BatteryLevel = 50, BatteryCapacity = 98 }
        );

        _context.SaveChanges();
    }
}