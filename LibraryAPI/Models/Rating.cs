using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Required]
        public int? BookCopyId { get; set; }

        [Required]
        public string MemberId { get; set; } = "";

        [Range(1, 5, ErrorMessage = "Please, rate between 1 and 5.")]
        public int Score { get; set; }
       
        [JsonIgnore]
        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; }
        
        [JsonIgnore]
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }
    }
}
