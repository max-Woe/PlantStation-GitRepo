using DataAccess;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; // Required for Exception

namespace PlantStationAPI.Backend.Controllers
{
    /// <summary>
    /// API Controller responsible for handling CRUD operations and retrieval of <see cref="Station"/> entities.
    /// It provides endpoints to create new stations, and to fetch stations or station IDs based on various criteria.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StationController(IMeasurementRepo measurementRepo, ISensorRepo sensorRepo, IStationRepo stationRepo) : ControllerBase
    {
        private readonly ISensorRepo _sensorRepo = sensorRepo; 
        private readonly IMeasurementRepo _measurementRepo = measurementRepo; 
        private readonly IStationRepo _stationRepo = stationRepo; 

        /// <summary>
        /// Creates a new station entry in the database.
        /// </summary>
        /// <param name="station">The <see cref="Station"/> object to be created.</param>
        /// <returns>
        /// A <see cref="JsonResult"/> containing an <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the created <see cref="Station"/> object if successful.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the station's ID is not zero, or if the repository fails to create the station.</description></item>
        /// </list>
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> Create(Station station)
        {
            if (station.Id != 0)
            {
                return new JsonResult(BadRequest("The id of an new entry has to be 0!"));
            }

            Station stationFromDb = await _stationRepo.Create(station);

            if (stationFromDb == null)
            {
                return new JsonResult(BadRequest("Station could not be created!"));
            }

            return new JsonResult(Ok(stationFromDb));
        }

        /// <summary>
        /// Retrieves a single station by its unique identifier (ID).
        /// </summary>
        /// <param name="stationId">The unique integer ID of the station.</param>
        /// <returns>
        /// A <see cref="JsonResult"/> containing an <see cref="IActionResult"/>:
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the <see cref="Station"/> object if found.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if the ID is zero, no station is found, or an exception occurs during lookup.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("{stationId}")]
        public async Task<JsonResult> Get(int stationId)
        {
            if (stationId == 0)
            {
                return new JsonResult(BadRequest("The id of a station has to be greater than 0!"));
            }
            Station? stationFromDb;
            try
            {
                stationFromDb = await _stationRepo.GetById(stationId);
            }
            catch (Exception)
            {
                return new JsonResult(BadRequest($"No matching station by StationId: {stationId}"));
            }

            if (stationFromDb == null)
            {
                return new JsonResult(BadRequest($"No matching station by StationId: {stationId}!"));
            }

            return new JsonResult(Ok(stationFromDb));
        }

        /// <summary>
        /// Retrieves a list of all station entities stored in the database.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult"/> (which can contain a <see cref="JsonResult"/> or <see cref="OkObjectResult"/>):
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of <see cref="Station"/> objects if successful.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if no stations are found or an exception occurs during retrieval.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            List<Station> stations;
            try
            {
                stations = await _stationRepo.GetAll();
            }
            catch (Exception)
            {
                return new JsonResult(BadRequest($"No station found!"));
            }

            if (stations == null)
            {
                return new JsonResult(BadRequest($"No station found!"));
            }

            return Ok(stations);
        }

        /// <summary>
        /// Retrieves a list of all unique station IDs currently stored in the database.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult"/> (which can contain a <see cref="JsonResult"/> or <see cref="OkObjectResult"/>):
        /// <list type="bullet">
        /// <item><description><see cref="ControllerBase.Ok(object)"/> (200) with the list of integer station IDs if successful.</description></item>
        /// <item><description><see cref="ControllerBase.BadRequest(object)"/> (400) if no station IDs are found or an exception occurs during retrieval.</description></item>
        /// </list>
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GetIds()
        {
            List<int> stationIds;
            try
            {
                stationIds = await _stationRepo.GetAllIds();
            }
            catch (Exception)
            {
                return new JsonResult(BadRequest($"No station found!"));
            }
            return Ok(stationIds);
        }
    }
}