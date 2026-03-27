using MediatR;

using Microsoft.AspNetCore.Mvc;

using StuffTracker.Application.Locations.Commands.CreateHome;
using StuffTracker.Application.Locations.Commands.CreateLocation;
using StuffTracker.Application.Locations.Queries.GetHomeById;
using StuffTracker.Application.Locations.Queries.GetHomes;
using StuffTracker.Application.Locations.Queries.GetLocationsForHome;

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

    [HttpPost("locations")]
    public async Task<IActionResult> CreateLocation(CreateLocationCommand command)
    {
        // TODO: Should this endpoint be in this controller? 
        // TODO: Should we fetch the HomeID from the Route or from the command, and compare that this HomeID matches the Parent's HomeID?
        var newLocationId = await _mediator.Send(command);
        return Created();
    }

    [HttpGet("{id}/locations")]
    public async Task<IActionResult> GetLocationsForHome([FromRoute] Guid id)
    {
        var locations = await _mediator.Send(new GetLocationsForHomeQuery(id));
        return Ok(locations);
    }


}
