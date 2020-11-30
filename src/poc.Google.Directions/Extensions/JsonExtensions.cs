using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace poc.Google.Directions.Extensions
{
    public static class JsonExtensions
    {
        //https://weblog.west-wind.com/posts/2015/mar/31/prettifying-a-json-string-in-net
        //https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/
        public static string PrettifyJson(this string json)
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

        public static string PrettifyJson(this JsonDocument jsonDoc)
        {
            if (jsonDoc == null)
            {
                return string.Empty;
            }
            
            var options = new JsonWriterOptions
            {
                Indented = true
            };

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, options))
            {
                jsonDoc.WriteTo(writer);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static async Task<Stream> BuildUtf8StreamStream(this string json)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms, Encoding.UTF8);

            await writer.WriteAsync(json);

            await writer.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);

            return ms;
        }

        public static string SafeGetString(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var val))
                return val.GetString();

            return default;
        }

        public static int SafeGetInt32(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var val))
                return val.GetInt32();

            return default;
        }

        public static double SafeGetDouble(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var val))
                return val.GetDouble();

            return double.NaN;
        }
    }
}