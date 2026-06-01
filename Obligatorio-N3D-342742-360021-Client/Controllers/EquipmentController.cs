using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    public class EquipmentController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var equipment = _auxiliarHttp
                    .EnviarYDeserializar<List<EquipmentVM>>("api/v1/equipment", "GET")
                    ?? new List<EquipmentVM>();

                return View(equipment);
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al obtener el equipamiento.";
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
                _auxiliarHttp.EnviarSolicitud("api/v1/equipment", "POST", dto);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al crear el equipamiento.";
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
                _auxiliarHttp.EnviarSolicitud($"api/v1/equipment/{id}", "PUT", dto);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al actualizar el equipamiento.";
                return View();
            }
        }

        [HttpPost("Equipment/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/equipment/{id}", "DELETE");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al eliminar el equipamiento.";
                return RedirectToAction("Index");
            }
        }
    }
}
