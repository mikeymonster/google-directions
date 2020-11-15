using System.IO;
using System.Text;
using System.Text.Json;

namespace poc.Google.Directions.Extensions
{
    public static class JsonExtensions
    {
        //https://weblog.west-wind.com/posts/2015/mar/31/prettifying-a-json-string-in-net
        //https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/
        public static string PrettifyJsonString(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            var doc = JsonDocument.Parse(json);

            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                doc.WriteTo(writer);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}