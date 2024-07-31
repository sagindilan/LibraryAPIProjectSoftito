using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
	public class BookCopy
	{
        public int Id { get; set; }

  
        public bool Condition { get; set; } // Kopyanın hasarlı mı değil mi kontrolü için

        [Range(1,3)]
        public int Status { get; set; } // Rafta, ödünç alındı, rezerve edildi

        public int BookId { get; set; }
        public string LocationShelf { get; set; } = "";
        public string? Barcode { get; set; }

        public int Stock { get; set; } = 1;

        // Kitap kopyasına yapılan toplam oy sayısı
        public int VoteCount { get; set; }

        // Kitap kopyasının toplam puanı
        public int VoteSum { get; set; }

        // Kitap kopyasının ortalama puanı
        public double? Rating { get; set; }

        // İlişkiler
        [JsonIgnore]
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        [JsonIgnore]
        public List<BorrowingHistory>? BorrowingHistory { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(LocationShelf))]
        public Location? Location { get; set; }
        public List<Rating>? Ratings { get; set; }
    }
}

