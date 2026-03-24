using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StuffTracker.Domain.Exceptions
{
    public class NotFoundException(string resourceType, string resourceId) : Exception($"The requested {resourceType} with ID {resourceId} was not found.")
    {
    }
}