using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    public class LoansController(AuxiliarClienteHttp _auxiliarHttp) : Controller
    {
        public IActionResult Index()
        {
            try
            {
                var loans = _auxiliarHttp
                    .EnviarYDeserializar<List<LoanVM>>("api/v1/loans/pendingRequests", "GET")
                    ?? new List<LoanVM>();

                return View(loans);
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al obtener los préstamos.";
                return View(new List<LoanVM>());
            }
        }

        [HttpGet("Loans/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Loans/Create")]
        public IActionResult Create(int memberId, int equipmentId, string requestDate)
        {
            try
            {
                var dto = new CreateLoanDto
                {
                    MemberId = memberId,
                    EquipmentId = equipmentId,
                    RequestDate = requestDate
                };
                _auxiliarHttp.EnviarSolicitud("api/v1/loans/createLoan", "POST", dto);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al solicitar el préstamo.";
                return View();
            }
        }

        [HttpPost("Loans/Approve/{id}")]
        public IActionResult Approve(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/approve/{id}", "PUT");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al aprobar el préstamo.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Reject/{id}")]
        public IActionResult Reject(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/reject/{id}", "PUT");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al rechazar el préstamo.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Return/{id}")]
        public IActionResult Return(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/return/{id}", "POST");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al registrar la devolución.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/cancel/{id}", "POST");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al cancelar el préstamo.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/deleteLoanRequest/{id}", "DELETE");
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Error al eliminar la solicitud de préstamo.";
                return RedirectToAction("Index");
            }
        }
    }
}
