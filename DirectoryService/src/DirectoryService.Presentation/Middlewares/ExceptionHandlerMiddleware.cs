using DirectoryService.Shared;

namespace DirectoryService.Presentation.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await ExceptionHandleAsync(context, e);
        }
    }

    private async Task ExceptionHandleAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var error = Error.Failure("server.internal",  exception.Message);

        var envelope = Envelope.Error(new Errors([error]));

        await context.Response.WriteAsJsonAsync(envelope);
    }
}