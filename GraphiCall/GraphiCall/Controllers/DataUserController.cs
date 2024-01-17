using Graphicall.Helpers;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Graphicall.Controllers
{
    [ApiController]
    [Route("manage")]
    public class DataUserController(
        ApplicationDbContext context,
        ILogger<DataUserController> logger) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<DataUserController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpPost("addData")]
        public async Task<IActionResult> PostData([FromBody] DataUser dataUser)
        {
            _logger.LogDebug("PostData called with user: {User}", dataUser);
            _logger.LogDebug($"Received data: {JsonSerializer.Serialize(dataUser)}");

            var validationResult = ValidateUserData(dataUser);
            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                await _context.DataUsers.AddAsync(dataUser);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user data.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private IActionResult ValidateUserData(DataUser userData)
        {
            // Define a dictionary of field names and their corresponding validation methods.
            #pragma warning disable CS8621 // THIS CAN BE NULL!
            var validations = new Dictionary<string, Func<string, (bool isValid, string errorMessage)>>
            {
                { nameof(userData.UserCountry), ValidationHelpers.IsCountryValid },
                { nameof(userData.UserStreet), ValidationHelpers.IsStreetValid },
                { nameof(userData.UserFirstName), ValidationHelpers.IsNameValid },
                { nameof(userData.UserSurname), ValidationHelpers.IsNameValid },
                { nameof(userData.UserTown), ValidationHelpers.IsTownValid },
                { nameof(userData.UserFlatNumber), ValidationHelpers.IsFlatNumberValid }
            };
            #pragma warning restore CS8621 //End of this shitty warning

            foreach (var validation in validations)
            {
                var propertyInfo = userData.GetType().GetProperty(validation.Key);

                if (propertyInfo == null)
                {
                    return BadRequest(new { error = $"Property {validation.Key} does not exist on DataUser." });
                }

                var fieldValue = propertyInfo.GetValue(userData, null)?.ToString();

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    var (isValid, errorMessage) = validation.Value(fieldValue);
                    if (!isValid)
                    {
                        return BadRequest(new { error = $"{validation.Key}: {errorMessage}" });
                    }
                }
            }

            return null; 
        }


    }
}
