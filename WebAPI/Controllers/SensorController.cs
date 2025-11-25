using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlantStationAPI.Backend.Controllers
{
    /// <summary>
    /// API Controller for handling operations related to <see cref="Sensor"/> entities.
    /// It provides endpoints for creating sensors and retrieving sensor information based on ID, station ID, or fetching all sensors/IDs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SensorController(IMeasurementRepo measurementRepo, ISensorRepo sensorRepo, IStationRepo stationRepo) : ControllerBase
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo; 
        private readonly IMeasurementRepo _measurementRepo = measurementRepo; 
        private readonly IStationRepo _stationRepo = stationRepo;

        /// <summary>
        /// Creates a new sensor entry in the database.
        /// </summary>
        /// <param name="sensor">The <see cref="Sensor"/> object to be created.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) if creation is successful.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the sensor's ID is not zero (indicating it's not a new entity).</description></item>
        /// <item><description><see cref="ControllerBase.Conflict(object)"/> (409) if a database conflict occurs (e.g., uniqueness constraint violation).</description></item>
        /// </list>
        /// </returns>
        [HttpPost]
        public IActionResult Create(Sensor sensor)
        {
            if (sensor.Id != 0)
            {
                return BadRequest("The id of an new entry has to be 0!");
            }

            try
            {
                _sensorRepo.Create(sensor);
            }
            catch (DbUpdateException)
            {
                return Conflict("A conflicting sensor already exists.");
            }

            return Ok(sensor);
        }

        /// <summary>
        /// Retrieves a single sensor by its unique identifier (ID).
        /// </summary>
        /// <param name="id">The unique integer ID of the sensor.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the <see cref="Sensor"/> object if found.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the ID is not greater than zero.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no sensor with the given ID exists.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            Sensor? sensor;

            if (id <= 0)
            {
                return BadRequest("The id has to be greater than 0.");
            }

            sensor = await _sensorRepo.GetById(id);

            if (sensor == null)
            {
                return NotFound();
            }

            return Ok(sensor);
        }

        /// <summary>
        /// Retrieves a list of all unique sensor IDs currently stored in the database.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of integer sensor IDs.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no sensor IDs are found (or the returned list is null).</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetIds()
        {
            List<int> sensors = await _sensorRepo.GetAllIds();

            if (sensors == null) 
            {
                return NotFound();
            }

            return Ok(sensors);
        }

        /// <summary>
        /// Retrieves a list of sensor IDs associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The ID of the station whose sensor IDs are to be retrieved.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of integer sensor IDs.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no sensor IDs are found for the given station ID (or the returned list is null).</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetIdsByStationId(int stationId)
        {
            List<int> sensors = await _sensorRepo.GetIdsByStationId(stationId);

            if (sensors == null)
            {
                return NotFound();
            }

            return Ok(sensors);
        }
        /// <summary>
        /// Retrieves a list of sensor IDs associated with a specific station ID.
        /// </summary>
        /// <param name="stationId">The ID of the station whose sensor IDs are to be retrieved.</param>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of integer sensor IDs.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no sensor IDs are found for the given station ID (or the returned list is null).</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetByStationId(int stationId)
        {
            List<Sensor> sensors = await _sensorRepo.GetByStationId(stationId);

            if (sensors == null)
            {
                return NotFound();
            }

            return Ok(sensors);
        }

        /// <summary>
        /// Retrieves a list of all sensor entities stored in the database.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of <see cref="Sensor"/> objects.</description></item>
        /// <item><description><see cref="ControllerBase.NotFound"/> (404) if no sensors are found (or the returned list is null).</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAllSensors()
        {
            List<Sensor> sensors = await _sensorRepo.GetAll();

            if (sensors == null)
            {
                return NotFound();
            }

            return Ok(sensors);
        }
    }
}