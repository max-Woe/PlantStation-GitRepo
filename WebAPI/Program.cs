using Microsoft.EntityFrameworkCore;
using DataAccess;
using Serilog;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using LoggingService;
using DataAccess.Repositories;
using DataAccess.Interfaces;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/PlantstationApiErrorLog.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://0.0.0.0:5000");

var connectionString = builder.Configuration["ConnectionString:PlantStationDbRaspi"];
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("!!!! FEHLER: Der Verbindungsstring 'PlantStationDbRaspi' wurde nicht in der Konfiguration gefunden.");
    throw new InvalidOperationException("Der erforderliche Verbindungsstring fehlt.");
}

builder.Services.AddDbContext<IApiContext, ApiContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddScoped<ILoggingService, SeriLoggingService>();
builder.Services.AddScoped<ISensorRepo, SensorRepo>();
builder.Services.AddScoped<IStationRepo, StationRepo>();
builder.Services.AddScoped<IMeasurementRepo, MeasurementRepo>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7186", "http://localhost:5283")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazor");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
