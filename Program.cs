using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using webapi_demo.CustomMiddleware;
using webapi_demo.Data;
using webapi_demo.Interfaces;
using webapi_demo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IToDoService, ToDoService>();
builder.Services.AddDbContext<ToDoDbContext>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services
    .AddControllers()
    .AddNewtonsoftJson();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<LoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
