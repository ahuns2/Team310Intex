using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    public static T GetObject<T>(this ISession session, string key)
    {
        var data = session.GetString(key);
        return data == null ? default(T) : JsonConvert.DeserializeObject<T>(data);
    }

    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }
}