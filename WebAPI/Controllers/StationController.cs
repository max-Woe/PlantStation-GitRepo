using DataAccess;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;

namespace PlantStationAPI.Backend.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class StationController : ControllerBase
{
    private readonly ApiContext _context;

    public StationController(ApiContext context)
    {
        _context = context;
    }

    [HttpPost]
    public JsonResult Create(Station station)
    {
        if (station.Id != 0)
        {
            return new JsonResult(BadRequest("The id of an new entry has to be 0!"));
        }

        _context.Stations.Add(station);
        _context.SaveChanges();

        return new JsonResult(Ok(station));
    }

    [HttpGet("{stationId}")]
    public JsonResult GetStation(int stationId) 
    {
        if (stationId == 0)
        {
            return new JsonResult(BadRequest("The id of a station has to be greater than 0!"));
        }
        Station station;
        try
        {
            station = _context.Stations.Find(stationId);
        }
        catch (Exception)
        {
            return new JsonResult(BadRequest($"No matching station by StationId: {stationId}"));
        }

        return new JsonResult(Ok(station)); 
    }

    [HttpGet]
    public ActionResult GetAllStations() 
    {
        List<Station> stations;
        try
        {
            stations = _context.Stations.ToList();
        }
        catch (Exception)
        {
            return new JsonResult(BadRequest($"No station found!"));
        }

        return Ok(stations); 
    }
}