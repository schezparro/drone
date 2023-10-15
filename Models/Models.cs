using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public enum DroneState { IDLE, LOADING, LOADED, DELIVERING, DELIVERED, RETURNING }
public enum DroneModel { Lightweight, Middleweight, Cruiserweight, Heavyweight }

public class Drone
{
    [Key]
    [MaxLength(100)]
    public string SerialNumber { get; set; }

    [Required]
    public DroneModel Model { get; set; }

    [Range(0, 500)]
    public int WeightLimit { get; set; }

    [Range(0, 100)]
    public int BatteryCapacity { get; set; }


    [Range(0, 100)]
    public int BatteryLevel { get; set; }

    [Required]
    public DroneState State { get; set; }

    public ICollection<Medication> Medications { get; set; }

}

public class Medication
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"^[A-Za-z0-9-_]+$")]
    public string Name { get; set; }

    [Range(0, 500)]
    public int Weight { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9_]+$")]
    public string Code { get; set; }

    public string Image { get; set; }

    public string DroneSerialNumber { get; set; }
    [JsonIgnore]
    public Drone Drone { get; set; }
}

public class EventLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string DroneSerial { get; set; }

    public DateTime Timestamp { get; set; }

    public int BatteryLevel { get; set; }
}

public class DroneDTO
{
    [Key]
    [MaxLength(100)]
    public string SerialNumber { get; set; }

    [Required]
    public string Model { get; set; }

    [Range(0, 500)]
    public int WeightLimit { get; set; }

    [Range(0, 100)]
    public int BatteryCapacity { get; set; }


    [Range(0, 100)]
    public int BatteryLevel { get; set; }

    [Required]
    public string State { get; set; }

    public ICollection<MedicationDTO> Medications { get; set; }
}

public class MedicationDTO
{
    [Required]
    [RegularExpression(@"^[A-Za-z0-9-_]+$")]
    public string Name { get; set; }

    [Range(0, 500)]
    public int Weight { get; set; }

    [Required]
    [RegularExpression(@"^[A-Z0-9_]+$")]
    public string Code { get; set; }

    public string Image { get; set; }
}