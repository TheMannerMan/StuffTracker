using FluentValidation;

using MediatR;

namespace StuffTracker.Application.Common.Behaviors;

// This is a MediatR pipeline behavior, meaning it runs automatically for every
// command/query that passes through MediatR — before the actual handler executes.
//
// Its job is to validate the incoming request using any FluentValidation validators
// registered for that request type. If validation fails, it throws a ValidationException
// and the handler never runs.
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // If no validators are registered for this request type, skip validation entirely
        // and just pass the request along to the next step in the pipeline.
        if (!validators.Any())
            return await next(cancellationToken);

        // Wrap the request in a ValidationContext so FluentValidation can inspect it.
        var context = new ValidationContext<TRequest>(request);

        // Run all validators asynchronously and in parallel.
        // ValidateAsync (instead of Validate) is important here: it supports both
        // sync and async rules. Using the sync Validate() would silently skip any
        // async rules (e.g. checking uniqueness against the database).
        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // If any validation rules failed, throw an exception with all the errors.
        // This stops execution — the actual handler will not be called.
        if (failures.Count != 0)
            throw new ValidationException(failures);

        // All validation passed — continue to the next step (the actual handler).
        return await next(cancellationToken);
    }
}
