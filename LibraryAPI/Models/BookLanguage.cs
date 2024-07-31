using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryAPI.Models
{
	public class BookLanguage
	{
        public int BookId { get; set; }

        public string LanguageId { get; set; } = "";

        [JsonIgnore]
        [ForeignKey(nameof(LanguageId))]
        public Language? Language { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }
    }
}

