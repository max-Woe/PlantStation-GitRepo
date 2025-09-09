using System.Reflection;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using LoggingService;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class MeasurementCollectionController(IMeasurementRepo measurementRepo, ISensorRepo sensorRepo, IStationRepo stationRepo) : ControllerBase
{
    //private readonly ApiContext _context = context;
    //private readonly ILoggingService _logger = logger;
    private readonly ISensorRepo _sensorRepo = sensorRepo;
    private readonly IMeasurementRepo _measurementRepo = measurementRepo;
    private readonly IStationRepo _stationRepo = stationRepo;


    [HttpPost]
    public async Task<IActionResult> ReceiveMeasurementCollection(MeasurementCollection measurementCollction)
    {
        // TODO: Prüfen ob Station existiert, wenn nicht anlegen oder stationId bei Create setzen
        // TODO: Prüfen ob Sensoren existieren, wenn nicht anlegen oder sensorId bei Create setzen
        if (_measurementRepo is MeasurementRepo measurementRepo)
        {
            List<Measurement> collectedMeasurementsList = ConvertCollectionToMeasurementsList(measurementCollction);
            //Measurement createdMeasurement;
            List<Sensor> sensors;
            if (_stationRepo is StationRepo stationRepo&& _sensorRepo is SensorRepo sensorRepo)
            {
                Station station = await stationRepo.GetByMacAdress(measurementCollction.MacAddress);

                if (station!= null)
                {
                    int i = 0;
                    foreach (Measurement measurement in collectedMeasurementsList)
                    {
                        await measurementRepo.Create(measurement, measurementCollction.MacAddress);
                        i++;
                    }
                }
            }
        
            return Ok(measurementCollction);
        }
        return BadRequest(measurementCollction);
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveMeasurement(ReceivedMeasurement receivedMeasurement)
    {
        if(_stationRepo is StationRepo stationRepo && _sensorRepo is SensorRepo sensorRepo && _measurementRepo is MeasurementRepo measurementRepo)
        {
            Station? station = await stationRepo.GetByMacAdress(receivedMeasurement.MacAddress);

            if (station == null)
            {
                station = await _stationRepo.Create(new Station()
                                {
                                    Id = 0,
                                    MacAddress = receivedMeasurement.MacAddress,
                                    CreatedAt = DateTime.UtcNow,
                                    SensorsCount = 0
                                });
            }
            
            List<Sensor> sensors = await sensorRepo.GetListByStationId(station.Id);

            //TODO: statt auf Einheit zu prüfen, besser einen Typ dem Übermittelten Messwert(Arduino) und dem Sensor(API) hinzufügen 
            if(!sensorRepo.CheckForSensorByStationIdAndUnit(station.Id, receivedMeasurement.Type).Result)
            {
                sensors.Add(await _sensorRepo.Create(new Sensor()
                {
                    Id = 0,
                    Unit = receivedMeasurement.Type,
                    DeviceId = 99999,
                    StationId = station.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }));
            }

            foreach (Sensor sensor in sensors)
            {
                if (sensor.Type == receivedMeasurement.Type)
                {
                    Measurement measurement = new Measurement()
                    {
                        Id = 0,
                        Value = receivedMeasurement.Value,
                        Type = receivedMeasurement.Type,
                        Unit = receivedMeasurement.Unit,
                        SensorId = sensor.Id,
                        SensorIdReference = 99999,
                        RecordedAt =receivedMeasurement.Time,
                        CreatedAt = DateTime.UtcNow
                    };

                    await measurementRepo.Create(measurement, receivedMeasurement.MacAddress);

                    return Ok(receivedMeasurement);
                }
            }
        }
        return BadRequest(receivedMeasurement);
    }
    private List<Measurement> ConvertCollectionToMeasurementsList(MeasurementCollection measurementCollction)
    {
        List<Measurement> measurements = new List<Measurement>();
        Measurement measurement;

        bool hasChanged;

        foreach (PropertyInfo prop in measurementCollction.GetType().GetProperties())
        {
            hasChanged = false;
            measurement = new Measurement()
            {
                Id = 0,
                SensorIdReference = 1,
                RecordedAt = measurementCollction.Time.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            };
            switch (prop.Name)
            {
                case "WaterLevel":
                    measurement.Value = measurementCollction.WaterLevel;
                    measurement.Unit = "%";
                    measurement.SensorId = 18;
                    hasChanged = true;
                    break;
                case "Moisture":
                    measurement.Value = measurementCollction.Moisture;
                    measurement.Unit = "%";
                    measurement.SensorId = 19;
                    hasChanged = true;
                    break;
                case "Temperature":
                    measurement.Value = measurementCollction.Temperature;
                    measurement.Unit = "°C";
                    measurement.SensorId = 20;
                    hasChanged = true;
                    break;
                case "Humidity":
                    measurement.Value = measurementCollction.Humidity;
                    measurement.Unit = "%rel";
                    measurement.SensorId = 21;
                    hasChanged = true;
                    break;
            }
            if (hasChanged)
            {
                measurements.Add(measurement);
            }
        }

        //measurements.Add(new Measurement()
        //{
        //    Id = 0,
        //    Value = measurementCollction.WaterLevel,
        //    SensorId = 1,
        //    SensorIdReference = 1,
        //    RecordedAt = measurementCollction.Time,
        //    CreatedAt = DateTime.UtcNow
        //});

        //measurements.Add(new Measurement()
        //{
        //    Id = 0,
        //    Value = measurementCollction.Moisture,
        //    SensorId = 2,
        //    SensorIdReference = 1,
        //    RecordedAt = measurementCollction.Time,
        //    CreatedAt = DateTime.UtcNow
        //});

        //measurements.Add(new Measurement()
        //{
        //    Id = 0,
        //    Value = measurementCollction.Temperature,
        //    SensorId = 3,
        //    SensorIdReference = 1,
        //    RecordedAt = measurementCollction.Time,
        //    CreatedAt = DateTime.UtcNow
        //});

        //measurements.Add(new Measurement()
        //{
        //    Id = 0,
        //    Value = measurementCollction.Humidity,
        //    SensorId = 4,
        //    SensorIdReference = 1,
        //    RecordedAt = measurementCollction.Time,
        //    CreatedAt = DateTime.UtcNow
        //});

        return measurements;
    }
}
