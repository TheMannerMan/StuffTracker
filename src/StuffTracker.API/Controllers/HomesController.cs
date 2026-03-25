using MediatR;

using Microsoft.AspNetCore.Mvc;

using StuffTracker.Application.Locations.Commands.CreateHome;
using StuffTracker.Application.Locations.Queries.GetHomeById;
using StuffTracker.Application.Locations.Queries.GetHomes;

namespace StuffTracker.API.Controllers;

[ApiController]
[Route("api/homes")]
public class HomesController(IMediator _mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateHome(CreateHomeCommand command)
    {
        Guid id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetHomeById), new { id }, null);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHomeById([FromRoute] Guid id)
    {
        var home = await _mediator.Send(new GetHomeByIdQuery(id));
        return Ok(home);
    }

    [HttpGet]
    public async Task<IActionResult> GetHomes()
    {
        var homes = await _mediator.Send(new GetHomesQuery());
        return Ok(homes);
    }
}
