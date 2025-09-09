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

var connectionString = "Server=192.168.178.75;Database=plantstationdatabase;Username=maxpowa;Password=Marianne1967;";
//var connectionString = "Server=localhost\\SQLEXPRESS;Database=plantstationdatabase;Trusted_Connection=True;TrustServerCertificate=True;";// Environment.GetEnvironmentVariable("MSSM_CONN_STRING");//"Server = localhost\\SQLEXPRESS; Database = plantStationdb; Trusted_Connection = True;";//
Console.WriteLine("!!!! AKTUELLER CONNECTION STRING: " + connectionString);

//// Für SQL Server muss hier UseSqlServer stehen!
//builder.Services.AddDbContext<ApiContext>(opt =>
//    opt.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApiContext>(opt =>
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
