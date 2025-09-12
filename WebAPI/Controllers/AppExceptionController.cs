using Microsoft.AspNetCore.Mvc;
using DataAccess;
using DataAccess.Models;

namespace PlantStationAPI.Backend.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class AppExceptionController : ControllerBase
{
    private readonly ApiContext _context;

    public AppExceptionController(ApiContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult Create(AppException appException)
    {
        if (appException.id != 0)
        {
            return BadRequest("The id of an new entry has to be 0!");
        }

        _context.AppExceptions.Add(appException);
        _context.SaveChanges();

        return Ok(appException);
    }
}