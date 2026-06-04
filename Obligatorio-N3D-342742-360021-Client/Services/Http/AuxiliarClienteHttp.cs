using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Error = Obligatorio_N3D_342742_360021_Client.Models.Error;

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
            string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            Error error = JsonSerializer.Deserialize<Error>(json, opts);
            throw new Exception(error.Message);

        }

        public string ObtenerBody(HttpResponseMessage respuesta)
        {
            return respuesta.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        public T? EnviarYDeserializar<T>(string relativeUrl, string verbo, object? body = null, string? token = null)
        {
            var resp = EnviarSolicitud(relativeUrl, verbo, body, token);
            var json = ObtenerBody(resp);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Deserialize<T>(json, opts);
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