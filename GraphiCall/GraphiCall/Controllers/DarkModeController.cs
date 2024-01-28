using Microsoft.AspNetCore.Mvc;

namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("darkmode")]
    public class DarkModeController : ControllerBase
    {
        private static bool _isDarkMode = false; // Domyślnie false

        [HttpGet("getDark")]
        public ActionResult<bool> GetDarkMode()
        {
            return _isDarkMode;
        }

        [HttpPost("toggleMode")]
        public IActionResult SetDarkMode([FromBody] bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
            return Ok();
        }
    }
}
