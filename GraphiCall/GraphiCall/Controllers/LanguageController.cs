using Microsoft.AspNetCore.Mvc;

namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("language")]
    public class LanguageController : ControllerBase
    {
        private static bool _isTranslatedLang = false; 

        [HttpGet("getLang")]
        public ActionResult<bool> GetLang()
        {
            return _isTranslatedLang;
        }

        [HttpPost("toggleLang")]
        public IActionResult SetLang([FromBody] bool isTranslatedLang)
        {
            _isTranslatedLang = isTranslatedLang;
            return Ok();
        }

    }
}
