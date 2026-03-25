using StuffTracker.Application.Extensions;
using StuffTracker.Infrastructure.Extensions;
using StuffTracker.API.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();
builder.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
