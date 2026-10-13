using System.Text.RegularExpressions;

namespace BankAccounts.Api.Middleware;

public sealed partial class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    private const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadOrCreate(context.Request);
        context.Items[ItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    public static string Get(HttpContext context) =>
       context.Items.TryGetValue(ItemKey, out var value) && value is string id ? id : context.TraceIdentifier;    
    private static string ReadOrCreate(HttpRequest request)
    {
        var supplied = request.Headers[HeaderName].ToString();
        return SafeCorrelationId().IsMatch(supplied) ? supplied : Guid.NewGuid().ToString();
    }

    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$")]
    private static partial Regex SafeCorrelationId();
}
