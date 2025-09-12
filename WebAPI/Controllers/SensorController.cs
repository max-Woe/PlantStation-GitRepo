using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PlantStationAPI.Backend.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class SensorController : ControllerBase
{
    private readonly ApiContext _context;

    public SensorController(ApiContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Create(Sensor sensor)
    {
        if (sensor.Id != 0)
        {
            return BadRequest("The id of an new entry has to be 0!");
        }

        try
        {
            _context.Sensors.Add(sensor);
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            return Conflict("A conflicting sensor already exists.");
        }

        return Ok(sensor);
    }

    [HttpGet]
    public IActionResult Get(int id)
    {
        Sensor sensor;

        if (id <= 0)
        {
            return BadRequest("The id has to be greater than 0.");
        }


        sensor = _context.Sensors.Find(id);

        if (sensor == null)
        {
            return NotFound();
        }

        return Ok(sensor);
    }

    [HttpGet]
    public IActionResult GetAllSensors()
    {
        List<Sensor> sensors;
        
        sensors = _context.Sensors.ToList();

        if (sensors == null)
        {
            return NotFound();
        }

        return Ok(sensors);
    }
}