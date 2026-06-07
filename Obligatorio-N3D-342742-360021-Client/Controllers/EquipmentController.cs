using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
    public class EquipmentController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public async Task<IActionResult> Index(string? type)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                var equipment = _auxiliarHttp
                    .EnviarYDeserializar<List<EquipmentVM>>("api/v1/equipment", "GET", token: token)
                    ?? new List<EquipmentVM>();

                if (!string.IsNullOrWhiteSpace(type) && type != "All")
                    equipment = equipment.Where(e => e.Type == type).ToList();

                ViewBag.Type = type ?? "All";
                return View(equipment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching equipment list: {ex.Message}");
                ViewBag.msg = "The equipment list drifted off. Give it another moment.";
                return View(new List<EquipmentVM>());
            }
        }

        [HttpGet("Equipment/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Equipment/Create")]
        public IActionResult Create(string name, string description, string type, int quantity,
            double? focalLenght, string? mountType, string? sensorType)
        {
            try
            {
                var dto = new CreateEquipmentDto
                {
                    Name = name,
                    Description = description,
                    Type = type,
                    Quantity = quantity,
                    FocalLenght = focalLenght,
                    MountType = mountType,
                    SensorType = sensorType
                };
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud("api/v1/equipment", "POST", dto, token);
                TempData["Success"] = "Item added to the inventory.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Something got lost in transit. The item couldn't be added.";
                return View();
            }
        }

        [HttpGet("Equipment/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            // TODO: GET /api/v1/equipment/{id} when endpoint is available
            return View(new EquipmentVM { Id = id });
        }

        [HttpPost("Equipment/Edit/{id}")]
        public IActionResult Edit(int id, string name, string description, string equipmentType, int quantity,
            double? focalLenght, string? mountType, string? sensorType)
        {
            try
            {
                var dto = new UpdateEquipmentDto
                {
                    Name = name,
                    Description = description,
                    EquipmentType = equipmentType,
                    Quantity = quantity,
                    FocalLenght = focalLenght,
                    MountType = mountType,
                    SensorType = sensorType
                };
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/equipment/{id}", "PUT", dto, token);
                TempData["Success"] = "Equipment updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The update got lost out there. Please try again.";
                return View();
            }
        }

        [HttpPost("Equipment/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _auxiliarHttp.EnviarSolicitud($"api/v1/equipment/{id}", "DELETE", token: token);
                TempData["Success"] = "Item removed from inventory.";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.msg = "Couldn't remove that item. Something crossed our path.";
                Console.WriteLine(e.Message); // e.Message ahora contendrá el texto crudo si no era JSON
                return RedirectToAction("Index");
            }
        }
    }
}
