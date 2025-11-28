using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MineSweeper_MVC.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
