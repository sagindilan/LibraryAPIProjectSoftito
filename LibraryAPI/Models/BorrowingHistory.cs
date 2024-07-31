using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
	public class BorrowingHistory
	{
        public const int PenaltyPerDay = 5; // Günlük ceza tutarı
        public const int DamagePenalty = 50; // Hasar cezası

        public int Id { get; set; }
        public int BookCopyId { get; set; }
        public string MemberId { get; set; } = "";
        public string EmployeeId { get; set; } = "";
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; } 
        public DateTime? ReturnDate { get; set; }
        public int PenaltyAmount { get; set; } = 0;
        public bool IsDamaged { get; set; }


        [JsonIgnore]
        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }
        
        [JsonIgnore]
        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; }

    }
}

