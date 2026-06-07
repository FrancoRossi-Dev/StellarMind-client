using Obligatorio_N3D_342742_360021_Client.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Obligatorio_N3D_342742_360021_Client.Services.Http
{
    public class AuxiliarClienteHttp
    {
        private readonly IHttpClientFactory _factory;

        public AuxiliarClienteHttp(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public HttpResponseMessage EnviarSolicitud(string relativeUrl, string verbo, object? body = null, string? token = null)
        {
            var client = _factory.CreateClient("Api");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage resp = verbo.ToUpper() switch
            {
                "GET" => client.GetAsync(relativeUrl).GetAwaiter().GetResult(),
                "POST" => client.PostAsync(relativeUrl, CreateJsonContent(body)).GetAwaiter().GetResult(),
                "PUT" => client.PutAsync(relativeUrl, CreateJsonContent(body)).GetAwaiter().GetResult(),
                "DELETE" => client.DeleteAsync(relativeUrl).GetAwaiter().GetResult(),
                _ => throw new ArgumentException("Verbo no soportado", nameof(verbo))
            };

            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }

            // Leer body y devolver mensaje comprensible aunque no sea JSON
            var content = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"HTTP Error { (int)resp.StatusCode } for {relativeUrl}. Body: {content}");

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                Error? error = JsonSerializer.Deserialize<Error>(content, opts);
                if (error != null && !string.IsNullOrWhiteSpace(error.Message))
                    throw new Exception(error.Message);
                // si la deserialización no produjo un Error útil, lanzar el body crudo
                throw new Exception(content);
            }
            catch (JsonException)
            {
                // No es JSON: lanzar el texto crudo para que el caller lo vea
                throw new Exception(content);
            }
        }

        public string ObtenerBody(HttpResponseMessage respuesta)
        {
            return respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public T? EnviarYDeserializar<T>(string relativeUrl, string verbo, object? body = null, string? token = null)
        {
            var resp = EnviarSolicitud(relativeUrl, verbo, body, token);
            var json = ObtenerBody(resp) ?? string.Empty;
            Console.WriteLine($"Body recibido: {json}");

            if (string.IsNullOrWhiteSpace(json))
                return default;

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                return JsonSerializer.Deserialize<T>(json, opts);
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"Failed to deserialize JSON to {typeof(T).FullName}: {jex.Message}");
                // opcional: relanzar o devolver default; relanzar ayuda a detectar el problema en tiempo de ejecución
                throw;
            }
        }

        private static HttpContent? CreateJsonContent(object? obj)
        {
            if (obj == null)
                return null;
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}