using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DroneDbContext>();
builder.Services.AddEntityFrameworkInMemoryDatabase();
builder.Services.AddHangfire(config => { config.UseMemoryStorage(); });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();
app.UseHangfireServer();


using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DroneDbContext>();
    dbContext.Database.EnsureCreated();
}
RecurringJob.AddOrUpdate<DroneController>(x => x.CheckBatteryLevels(), Cron.Hourly);

app.Run();
