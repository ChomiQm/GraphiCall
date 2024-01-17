using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GraphiCall.Data
{
    public class Message
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromId { get; set; } = null!;
        public string ToId { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime SentOn { get; set; }

        [ForeignKey(nameof(FromId))]
        public virtual ApplicationUser? FromUser { get; set; }

        [ForeignKey(nameof(ToId))]
        public virtual ApplicationUser? ToUser { get; set; }
    }
}
