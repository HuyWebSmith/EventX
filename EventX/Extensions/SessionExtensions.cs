using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace EventX.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                MaxDepth = 64  // Đảm bảo độ sâu đủ lớn để xử lý các đối tượng phức tạp
            };

            session.SetString(key, JsonSerializer.Serialize(value, options));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null) return default;

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                MaxDepth = 64
            };

            return JsonSerializer.Deserialize<T>(value, options);
        }
    }
}
