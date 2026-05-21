using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StellarMinds.Shared.DTO.Users;
using StellarMinds.Shared.IUseCases.Users;
using StellarMinds.Shared.ViewModels;
using StellarMinds.BusinessLogic.Exceptions.LoginExceptions;

namespace StellarMinds.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private ICULoginUser<LogInUserVM> _loginUser;

        public LoginController(ICULoginUser<LogInUserVM> loginUser)
        {
            _loginUser = loginUser;
        }

        [HttpPost]
        public IActionResult Login(LogInUserVM LoginData)
        {
            try
            {
                LoggedUserDTO loggedUser = _loginUser.Execute(LoginData);
                return Ok(loggedUser);
            }
            catch(NoLoginDataException ex)
            {
                return StatusCode(400, ex.Message);
            }
            catch (InvalidLoginException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
