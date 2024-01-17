using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace GraphiCall.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime? UserDateOfUpdate { get; set; }

        [JsonIgnore]
        public virtual DataUser? DataUsers { get; set; }

        public virtual ICollection<Note> Notes { get; set; } = new HashSet<Note>();
        public virtual Calendar Calendar { get; set; } = null!;
        public virtual Whiteboard Whiteboard { get; set; } = null!;
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}
