using Microsoft.EntityFrameworkCore;
using Bogus;

public class DroneDbContext : DbContext
{
    public DroneDbContext(DbContextOptions<DroneDbContext> options)
   : base(options)
    {
    }
    public DbSet<Drone> Drones { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("drones");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medication>()
            .HasOne(m => m.Drone)
            .WithMany(d => d.Medications)
            .HasForeignKey(m => m.DroneSerialNumber);

        var droneModels = Enum.GetNames(typeof(DroneModel));
        var droneStates = Enum.GetNames(typeof(DroneState));
        var drones = new Faker<Drone>()
             .RuleFor(d => d.SerialNumber, f => f.Random.AlphaNumeric(10))
             .RuleFor(d => d.Model, f => f.Random.Enum<DroneModel>())
             .RuleFor(d => d.WeightLimit, f => f.Random.Number(0, 500))
             .RuleFor(d => d.BatteryCapacity, f => f.Random.Number(0, 100))
             .RuleFor(d => d.BatteryLevel, (f, d) => f.Random.Number(0, d.BatteryCapacity))
             .RuleFor(d => d.State, f => f.Random.Enum<DroneState>())
             .Generate(10);

        modelBuilder.Entity<Drone>().HasData(drones);

        modelBuilder.Entity<Medication>().HasData(
            new Faker<Medication>()
                .RuleFor(m => m.Id, f => f.Random.Number(0, int.MaxValue))
                .RuleFor(m => m.Name, f => f.Random.Word())
                .RuleFor(m => m.Weight, f => f.Random.Number(0, 500))
                .RuleFor(m => m.Code, f => f.Random.AlphaNumeric(10))
                .RuleFor(m => m.Image, f => f.Internet.Avatar())
                .RuleFor(m => m.DroneSerialNumber, f => f.PickRandom(drones).SerialNumber)
                .Generate(10)
                );

        base.OnModelCreating(modelBuilder);
    }
}