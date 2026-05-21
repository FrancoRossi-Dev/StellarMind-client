using Microsoft.AspNetCore.Mvc;
using StellarMinds.Shared.DTO.Equipments;
using StellarMinds.Shared.IUseCases.Equipments;


namespace Obligatorio_N3D_342742_360021.Controllers
{
    public class EquipmentController : Controller
    {
        private IUCRegisterEquipment<CreateTelescopeDTO> _registerTelescope;
        private IUCRegisterEquipment<CreateCameraDTO> _registerCamera;
        private IUCRegisterEquipment<CreateEyepieceDTO> _registerEyepiece;
        private IUCRegisterEquipment<CreateMountDTO> _registerMount;

        public EquipmentController(
            IUCRegisterEquipment<CreateTelescopeDTO> registerTelescope,
            IUCRegisterEquipment<CreateCameraDTO> registerCamera,
            IUCRegisterEquipment<CreateEyepieceDTO> registerEyepiece,
            IUCRegisterEquipment<CreateMountDTO> registerMount)
        {
            _registerTelescope = registerTelescope;
            _registerCamera = registerCamera;
            _registerEyepiece = registerEyepiece;
            _registerMount = registerMount;
        }

        // POST: Equipment/CreateTelescope
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTelescope(CreateTelescopeDTO dto)
        {
            try
            {
                _registerTelescope.Execute(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                return View();
            }
        }

        // POST: Equipment/CreateCamera
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCamera(CreateCameraDTO dto)
        {
            try
            {
                _registerCamera.Execute(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                return View();
            }
        }

        // POST: Equipment/CreateEyepiece
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEyepiece(CreateEyepieceDTO dto)
        {
            try
            {
                _registerEyepiece.Execute(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                return View();
            }
        }

        // POST: Equipment/CreateMount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMount(CreateMountDTO dto)
        {
            try
            {
                _registerMount.Execute(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                return View();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
