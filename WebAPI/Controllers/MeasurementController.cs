using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantStationAPI.Backend.Controllers
{
    /// <summary>
    /// API Controller for handling CRUD operations and retrieval of <see cref="Measurement"/> data.
    /// It provides endpoints for creating new measurements and fetching existing measurements based on ID, sensor ID, and time criteria.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MeasurementController(IMeasurementRepo measurementRepo, ISensorRepo sensorRepo, IStationRepo stationRepo) : ControllerBase
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo;
        private readonly IMeasurementRepo _measurementRepo = measurementRepo;
        private readonly IStationRepo _stationRepo = stationRepo;

        /// <summary>
        /// Creates a new measurement entry in the database.
        /// </summary>
        /// <param name="measurement">The <see cref="Measurement"/> object to be created.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) if creation is successful.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the input measurement is null, has a non-zero ID, or the associated sensor ID is not found.</description></item>
        /// </list>
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create(Measurement measurement)
        {
            if (measurement == null)
            {
                return BadRequest("measurment can not be null!");
            }

            if (measurement.Id != 0)
            {
                return BadRequest("The id of an new entry has to be 0!");
            }

            var sensor = await _measurementRepo.Create(measurement);

            if (sensor == null)
            {
                return BadRequest($"Measurement can not be stored. No sensor with the ID: {measurement.SensorId} found.");
            }

            return Ok(measurement);
        }

        /// <summary>
        /// Retrieves a single measurement by its unique identifier (ID).
        /// </summary>
        /// <param name="id">The unique integer ID of the measurement.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the <see cref="Measurement"/> object if found.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the ID is not greater than zero.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no measurement with the given ID exists.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than 0");
            }

            var result = await _measurementRepo.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the latest measurements for a specific sensor.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose measurements are to be retrieved.</param>
        /// <param name="count">The maximum number of measurements to return. Defaults to 180.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of <see cref="Measurement"/> objects.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no measurements are found for the given sensor ID.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetLastOfSensor(int sensorId, int count = 180)
        {
            List<Measurement> result = await _measurementRepo.GetLastOfSensor(sensorId, count);

            if (result.IsNullOrEmpty())
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves all measurements for a specific sensor recorded since a given date and time.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor whose measurements are to be retrieved.</param>
        /// <param name="since">The minimum date and time from which measurements should be included.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of <see cref="Measurement"/> objects.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the 'since' parameter is not a valid date.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no measurements are found within the specified time frame.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetLastOfSensorSince(int sensorId, DateTime since)
        {
            DateTime utcSince = DateTime.SpecifyKind(since, DateTimeKind.Utc).AddHours(-1);
            if (since <= DateTime.MinValue)
            {
                return BadRequest("since must be a valid date");
            }

            var result = await _measurementRepo.GetLastOfSensorSince(sensorId, utcSince);

            if (result.IsNullOrEmpty())
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}