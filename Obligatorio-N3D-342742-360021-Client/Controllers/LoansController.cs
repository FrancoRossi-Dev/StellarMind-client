using Obligatorio_N3D_342742_360021_Client.Services.Http;
using Microsoft.AspNetCore.Mvc;
using Obligatorio_N3D_342742_360021_Client.Models;
using Obligatorio_N3D_342742_360021_Client.Filters;

namespace Obligatorio_N3D_342742_360021_Client.Controllers
{
    [LoggedUserFilter]
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
                ViewBag.msg = "The loan list seems to be orbiting somewhere else. Try again.";
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
                TempData["Success"] = "Loan request submitted.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The request got lost in the void. Please try again.";
                return View();
            }
        }

        [HttpPost("Loans/Approve/{id}")]
        public IActionResult Approve(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/approve/{id}", "PUT");
                TempData["Success"] = "Loan request approved.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't approve that loan. Something drifted off course.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Reject/{id}")]
        public IActionResult Reject(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/reject/{id}", "PUT");
                TempData["Success"] = "Loan request rejected.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't reject that loan. Something went sideways out there.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Return/{id}")]
        public IActionResult Return(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/return/{id}", "POST");
                TempData["Success"] = "Loan marked as returned.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The return couldn't be logged. Something crossed our path.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Loans/Cancel/{id}")]
        public IActionResult Cancel(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/cancel/{id}", "POST");
                TempData["Success"] = "Loan canceled.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "Couldn't cancel that loan. Try again in a moment.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("Loans/MyLoans")]
        [AccessFilter("Member")]
        public IActionResult MyLoans()
        {
            return View();
        }

        [HttpPost("Loans/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _auxiliarHttp.EnviarSolicitud($"api/v1/loans/deleteLoanRequest/{id}", "DELETE");
                TempData["Success"] = "Loan request removed.";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.msg = "The request couldn't be removed. Something got in the way.";
                return RedirectToAction("Index");
            }
        }
    }
}
