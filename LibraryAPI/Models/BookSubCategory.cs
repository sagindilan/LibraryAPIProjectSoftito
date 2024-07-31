using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
	public class BookSubCategory
	{
        public int BookId { get; set; }

        public short SubCategoryId { get; set; }

        // İlişkiler

        [JsonIgnore]
        [ForeignKey(nameof(SubCategoryId))]
        public SubCategory? SubCategory { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}

