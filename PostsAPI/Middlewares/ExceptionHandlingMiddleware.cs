namespace PostsAPI.Middlewares;
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //throw new Exception("I made this exception on purpose");
            await next.Invoke(context);
        }
        catch(Exception e)
        {
            logger.LogError(e, "Unhandled exception occured, {traceId}", context.TraceIdentifier);
            if (context.Response.HasStarted) throw;
            await Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error occurred",
                extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
        }
    }
}


public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
