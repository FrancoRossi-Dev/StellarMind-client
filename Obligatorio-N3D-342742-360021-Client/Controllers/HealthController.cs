using Microsoft.AspNetCore.Mvc;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    /// <summary>
    /// Same-origin proxy for the API health endpoint. The browser polls this while the
    /// API spins back up from Render's idle sleep, so the status card can show real
    /// progress without running into cross-origin restrictions on the API itself.
    /// </summary>
    public class HealthController(IHttpClientFactory _httpClientFactory) : Controller
    {
        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Check()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Health");
                using var resp = await client.GetAsync("health");
                return Json(new { awake = resp.IsSuccessStatusCode });
            }
            catch
            {
                // Timeout, DNS, connection reset — all mean "still not ready".
                return Json(new { awake = false });
            }
        }
    }
}
