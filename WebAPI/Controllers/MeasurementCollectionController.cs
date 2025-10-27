using System.Reflection;
using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using LoggingService;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    /// <summary>
    /// API Controller responsible for receiving and processing measurement data, both as single readings and as collections.
    /// It interacts with the data access layer to retrieve station and sensor information and persist new measurements.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MeasurementCollectionController(IMeasurementRepo measurementRepo, ISensorRepo sensorRepo, IStationRepo stationRepo) : ControllerBase
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo;
        private readonly IMeasurementRepo _measurementRepo = measurementRepo;
        private readonly IStationRepo _stationRepo = stationRepo;

        /// <summary>
        /// Receives a collection of measurements from a single station and processes them individually.
        /// </summary>
        /// <param name="measurementCollction">A data transfer object (DTO) containing a set of measurements (e.g., WaterLevel, Moisture) and the station's MacAddress and Time.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success (<see cref="ControllerBase.Ok(object)"/>) or failure (<see cref="ControllerBase.BadRequest(object)"/>).</returns>
        [HttpPost]
        public async Task<IActionResult> ReceiveMeasurementCollection(MeasurementCollection measurementCollction)
        {
            if (_measurementRepo is MeasurementRepo measurementRepo)
            {
                List<Measurement> collectedMeasurementsList = ConvertCollectionToMeasurementsList(measurementCollction);

                if (_stationRepo is StationRepo stationRepo && _sensorRepo is SensorRepo sensorRepo)
                {
                    Station station = await stationRepo.GetByMacAdress(measurementCollction.MacAddress);

                    if (station != null)
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

        /// <summary>
        /// Receives a single measurement reading and handles the persistence logic, including automatic creation of the station or sensor if they do not exist.
        /// </summary>
        /// <param name="receivedMeasurement">A DTO containing the measurement value, type, unit, timestamp, and the station's MacAddress.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success (<see cref="ControllerBase.Ok(object)"/>) or failure (<see cref="ControllerBase.BadRequest(object)"/>).</returns>
        [HttpPost]
        public async Task<IActionResult> ReceiveMeasurement(ReceivedMeasurement receivedMeasurement)
        {
            if (_stationRepo is StationRepo stationRepo && _sensorRepo is SensorRepo sensorRepo && _measurementRepo is MeasurementRepo measurementRepo)
            {
                Station? station = await stationRepo.GetByMacAdress(receivedMeasurement.MacAddress);
                Sensor? sensor;

                if (station == null)
                {
                    station = await _stationRepo.Create(new Station()
                    {
                        Id = 0,
                        MacAddress = receivedMeasurement.MacAddress,
                        CreatedAt = DateTime.UtcNow,
                        SensorsCount = 0
                    });

                    sensor = await _sensorRepo.Create(new Sensor()
                    {
                        Id = 0,
                        Unit = receivedMeasurement.Unit,
                        DeviceId = 99999,
                        Type = receivedMeasurement.Type,
                        StationId = station.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    sensor = await sensorRepo.GetSensorsByStationIdAndType(station.Id, receivedMeasurement.Type);

                    if (sensor == null)
                    {
                        sensor = await _sensorRepo.Create(new Sensor()
                        {
                            Id = 0,
                            Unit = receivedMeasurement.Unit,
                            DeviceId = 99999,
                            Type = receivedMeasurement.Type,
                            StationId = station.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                Measurement measurement = new Measurement()
                {
                    Id = 0,
                    Value = receivedMeasurement.Value,
                    Type = receivedMeasurement.Type, 
                    Unit = receivedMeasurement.Unit,
                    SensorId = sensor.Id,
                    SensorIdReference = 99999,
                    RecordedAt = receivedMeasurement.Time,
                    CreatedAt = DateTime.UtcNow
                };

                await measurementRepo.Create(measurement, receivedMeasurement.MacAddress);

                return Ok(receivedMeasurement);
            }
            return BadRequest(receivedMeasurement);
        }

        /// <summary>
        /// Private helper method to convert a single <see cref="MeasurementCollection"/> DTO, 
        /// which contains multiple measurement properties, into a <see cref="List{T}"/> of individual <see cref="Measurement"/> entities.
        /// </summary>
        /// <param name="measurementCollction">The source DTO containing various measurement values.</param>
        /// <returns>A list of <see cref="Measurement"/> entities created from the DTO's properties.</returns>
        /// <remarks>
        /// This method uses reflection (<see cref="System.Reflection.PropertyInfo"/>) and a switch statement 
        /// to map DTO properties (e.g., "WaterLevel") to specific <see cref="Measurement"/> entities, 
        /// assigning hardcoded sensor IDs and units.
        /// </remarks>
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
                        measurement.Type = "WaterLevel";
                        hasChanged = true;
                        break;
                    case "Moisture":
                        measurement.Value = measurementCollction.Moisture;
                        measurement.Unit = "%";
                        measurement.SensorId = 19;
                        measurement.Type = "Moisture";
                        hasChanged = true;
                        break;
                    case "Temperature":
                        measurement.Value = measurementCollction.Temperature;
                        measurement.Unit = "°C";
                        measurement.SensorId = 20;
                        measurement.Type = "Temperature";
                        hasChanged = true;
                        break;
                    case "Humidity":
                        measurement.Value = measurementCollction.Humidity;
                        measurement.Unit = "%rel";
                        measurement.SensorId = 21;
                        measurement.Type = "Humidity";
                        hasChanged = true;
                        break;
                        
                }

                if (hasChanged)
                {
                    measurements.Add(measurement);
                }
            }

            return measurements;
        }
    }
}