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
            var statusCode = (int)resp.StatusCode;
            Console.WriteLine($"HTTP Error {statusCode} for {relativeUrl}. Body: {content}");

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                Error? error = JsonSerializer.Deserialize<Error>(content, opts);
                if (error != null && !string.IsNullOrWhiteSpace(error.Message))
                    throw new ApiException(statusCode, error.Message);
            }
            catch (JsonException) { }

            throw new ApiException(statusCode, FriendlyMessage(statusCode));
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

        private static string FriendlyMessage(int statusCode) => statusCode switch
        {
            400 => "Something looks off with the details you entered. Please review and try again.",
            401 => "Your session has expired or you're not authorised. Please log in again.",
            403 => "You don't have permission to perform that action.",
            404 => "We couldn't find what you were looking for — it may have moved or been removed.",
            409 => "That action conflicts with existing data. Please check the details and try again.",
            422 => "Some of the information provided wasn't accepted. Please review and try again.",
            _ when statusCode >= 500 => "Something went wrong on our end. Please try again in a moment.",
            _ => "An unexpected error occurred. Please try again."
        };

        private static readonly JsonSerializerOptions _sendOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static HttpContent? CreateJsonContent(object? obj)
        {
            if (obj == null)
                return null;
            var json = JsonSerializer.Serialize(obj, _sendOpts);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}