using Microsoft.AspNetCore.Mvc;

namespace StuffTracker.API.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
