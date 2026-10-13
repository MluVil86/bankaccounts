using System.Text.Json;

namespace BankAccounts.Application.Events;

public static class EventSerializer
{
    public static string Serialize<TEvent>(TEvent @event) => JsonSerializer.Serialize(@event, JsonSerializerOptions.Web);
}
