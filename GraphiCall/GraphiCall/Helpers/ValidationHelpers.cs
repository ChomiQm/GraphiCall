using System.Text.RegularExpressions;

namespace Graphicall.Helpers
{
    public class ValidationHelpers
    {
        public static (bool isValid, string? errorMessage) IsPhoneNumberValid(string phoneNumber)
        {
            string phoneRegex = @"^\+\d{2,4} \d{3}-\d{3}-\d{3}$";
            if (Regex.IsMatch(phoneNumber, phoneRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invalid phone number format.");
            }
        }
        public static (bool isValid, string? errorMessage) IsEmailValid(string email)
        {
            string emailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w{2,}$";
            if (Regex.IsMatch(email, emailRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invalid email address format.");
            }
        }
        public static (bool isValid, string? errorMessage) IsPasswordValid(string password)
        {
            string passwordRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{8,16}$";
            if (Regex.IsMatch(password, passwordRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invalid password format.");
            }
        }
        public static (bool isValid, string? errorMessage) IsPriceValid(decimal price)
        {
            if (price > 0)
            {
                return (true, null);
            }
            else
            {
                return (false, "Price must be greater than 0.");
            }
        }
        public static (bool isValid, string? errorMessage) IsNameValid(string name)
        {
            string nameRegex = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ]+(?: [A-ZĄĆĘŁŃÓŚŹŻ][a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ]+)*$";

            if (Regex.IsMatch(name, nameRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invalid name format.");
            }
        }
        public static (bool isValid, string? errorMessage) IsTownValid(string text)
        {
            string townRegex = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\.\-]*(?<![\s\.\-])$";

            if (Regex.IsMatch(text, townRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invaild town format.");
            }

        }
        public static (bool isValid, string? errorMessage) IsStreetValid(string text)
        {
            string streetRegex = @"^[A-ZĄĆĘŁŃÓŚŹŻa-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ0-9\s\.\-]*$";

            if (Regex.IsMatch(text, streetRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invalid street format.");
            }
        }


        public static (bool isValid, string? errorMessage) IsCountryValid(string text)
        {
            string countryRegex = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-\']*$";

            if (Regex.IsMatch(text, countryRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invaild country format.");
            }

        }

        public static (bool isValid, string? errorMessage) IsFlatNumberValid(string text)
        {
            string flatRegex = @"^\d+[A-Za-z]?([-\/]\d*[A-Za-z]*)*$";
            if (Regex.IsMatch(text, flatRegex))
            {
                return (true, null);
            }
            else
            {
                return (false, "Invaild flat number format.");
            }

        }

    }
}
