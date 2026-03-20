using System;
using System.Collections.Generic;
using System.Text;

namespace StuffTracker.Application.Locations.Dtos;

internal class HomeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
