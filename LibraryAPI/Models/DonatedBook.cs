using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace LibraryAPI.Models
{
	public class DonatedBook
	{
        public int Id { get; set; }
        public int BookCopyId { get; set; }
        public string MemberId { get; set; } = "";
        public DateTime DonationDate { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; }

        
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }


    }
}

