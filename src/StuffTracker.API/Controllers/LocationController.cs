using MediatR;

using Microsoft.AspNetCore.Mvc;

using StuffTracker.Application.Locations.Commands.CreateHome;
using StuffTracker.Application.Locations.Queries.GetHome;

namespace StuffTracker.API.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController(IMediator _mediator) : Controller
{
    [HttpPost]
    public async Task<IActionResult> CreateHome(CreateHomeCommand command)
    {
        Guid id = await _mediator.Send(command);
        return Ok(id);

    }

    [HttpGet("homes/{id}")]
    public async Task<IActionResult> GetHomeById([FromRoute]Guid id)
    {
        var home = await _mediator.Send(new GetHomeByIdQuery(id));
        if (home is null)
        {
            return NotFound();
        }

        return Ok(home);
    }
}
