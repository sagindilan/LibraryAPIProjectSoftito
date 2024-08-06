using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
    public class PurchasedBook
    {
        public int Id { get; set; }

        public DateTime PurchaseDate { get; set; }

        public short Price { get; set; }
        public int PublisherId { get; set; }
        public string EmployeeId { get; set; } = ""; //USER tanımlaman lazım çünkü employee ıd yi sistemden login kişininkini almak lazım 
        public int BookCopyId { get; set; }

        // İlişkiler
        [JsonIgnore]
        [ForeignKey(nameof(PublisherId))]
        public Publisher? Publisher { get; set; } // Kitabı kütüphaneye satan yayıncı
        [JsonIgnore]
        [ForeignKey(nameof(EmployeeId))]
        public Employee? Employee { get; set; } // Kitabı satın alan çalışan
        [JsonIgnore]
        [ForeignKey(nameof(BookCopyId))]
        public BookCopy? BookCopy { get; set; } // Satın alınan kitap
    }
}

