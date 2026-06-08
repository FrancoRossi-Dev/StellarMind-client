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
        public IActionResult Create(string brand, string model, string type, int quantity,
            string? aperture, string? focalRatio, string? focalLenght, string? weight,
            string? resolution, string? sensorType, string? pixelSize,
            string? diameter, string? fieldOfView,
            string? mountType, string? weightSupport, bool isGoTo = false)
        {
            try
            {
                var dto = new CreateEquipmentDto
                {
                    Brand = brand,
                    Model = model,
                    Type = type,
                    Quantity = quantity,
                    AvailableQuantity = quantity,
                    Aperture = aperture,
                    FocalRatio = focalRatio,
                    FocalLenght = focalLenght,
                    Weight = weight,
                    Resolution = resolution,
                    SensorType = sensorType,
                    PixelSize = pixelSize,
                    Diameter = diameter,
                    FieldOfView = fieldOfView,
                    MountType = mountType,
                    WeightSupport = weightSupport,
                    IsGoTo = isGoTo
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
            try
            {
                var token = HttpContext.Session.GetString("Token");
                var all = _auxiliarHttp.EnviarYDeserializar<List<EquipmentVM>>("api/v1/equipment", "GET", token: token) ?? new();
                var item = all.FirstOrDefault(e => e.Id == id);
                if (item == null)
                {
                    TempData["Error"] = "That item seems to have drifted off the map.";
                    return RedirectToAction("Index");
                }
                return View(item);
            }
            catch (Exception)
            {
                TempData["Error"] = "Couldn't load that item. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Equipment/Edit/{id}")]
        public IActionResult Edit(int id, string brand, string model, string equipmentType, int quantity, int availableQuantity,
            string? aperture, string? focalRatio, string? focalLenght, string? weight,
            string? resolution, string? sensorType, string? pixelSize,
            string? diameter, string? fieldOfView,
            string? mountType, string? weightSupport, bool isGoTo = false)
        {
            try
            {
                var dto = new UpdateEquipmentDto
                {
                    Id = id,
                    Brand = brand,
                    Model = model,
                    EquipmentType = equipmentType,
                    Quantity = quantity,
                    AvailableQuantity = availableQuantity,
                    Aperture = aperture,
                    FocalRatio = focalRatio,
                    FocalLenght = focalLenght,
                    Weight = weight,
                    Resolution = resolution,
                    SensorType = sensorType,
                    PixelSize = pixelSize,
                    Diameter = diameter,
                    FieldOfView = fieldOfView,
                    MountType = mountType,
                    WeightSupport = weightSupport,
                    IsGoTo = isGoTo
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
