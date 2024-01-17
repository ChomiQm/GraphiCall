using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GraphiCall.Data
{
    public class DataUser
    {
        [Key]
        public string? UserDataId { get; set; }
        public string UserFirstName { get; set; } = null!;
        public string UserSurname { get; set; } = null!;
        public string UserCountry { get; set; } = null!;
        public string UserTown { get; set; } = null!;
        public string? UserStreet { get; set; }
        public int? UserHomeNumber { get; set; }
        public string? UserFlatNumber { get; set; }

        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        [JsonIgnore]
        public virtual ApplicationUser? User { get; set; }

        [NotMapped]
        public string UserHomeNumberString
        {
            get => UserHomeNumber?.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    UserHomeNumber = result;
                }
                else
                {
                    UserHomeNumber = null;
                }
            }
        }

    }
}
