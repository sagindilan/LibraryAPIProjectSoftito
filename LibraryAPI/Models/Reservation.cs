using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace LibraryAPI.Models

{
	public class Reservation
	{
        public int Id { get; set; }
        public string MemberId { get; set; } = "";
        public int BookCopyId { get; set; }



        public DateTime ReservationDate { get; set; }
        public bool IsActive { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }


    }
}

