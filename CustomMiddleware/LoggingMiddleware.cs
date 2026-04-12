using System;
using System.Threading.Tasks;

namespace webapi_demo.CustomMiddleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    
    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;    
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"Handling request: ${context.Request.Path}");
        await _next(context);
        Console.WriteLine($"Fisnished handling request: ${context.Request.Path}");
    }
}
