using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
	public class Book
	{
        public int Id { get; set; }

        [StringLength(13,MinimumLength = 10)]
        [Column(TypeName = "varchar(13)")]
        public string? ISBN { get; set; }

        [Required]
        [StringLength(2000)]
        public string Title { get; set; } = "";

        [Range(1,short.MaxValue)]
        public short PageCount { get; set; }

        [Range(-4000, 2100)]
        public short PublishingYear { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int PrintCount { get; set; }

        public bool Banned { get; set; }

        public int PublisherId { get; set; }

        [NotMapped]
        public List<long>? AuthorIds { get; set; }

        [StringLength(6, MinimumLength = 3)]
        [Column(TypeName = "varchar(6)")]

        [JsonIgnore]
        [ForeignKey(nameof(PublisherId))]
        public Publisher? Publisher { get; set; }

        [JsonIgnore]
        public List<BookSubCategory>? BookSubCategories { get; set; }
        
        [JsonIgnore]
        public List<BookLanguage>? BookLanguages { get; set; }

        [JsonIgnore]
        public List<AuthorBook>? AuthorBooks { get; set; }


    }
}

