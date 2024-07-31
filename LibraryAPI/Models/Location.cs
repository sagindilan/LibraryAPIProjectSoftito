using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using LibraryAPI.Data;

namespace LibraryAPI.Models
{
	public class Location
	{
        [Key]
        [Required]
        [StringLength(6, MinimumLength = 3)]
        [Column(TypeName = "varchar(6)")]
        public string Shelf { get; set; } = "";

        [JsonIgnore]
        public List<BookCopy>? BookCopy{ get; set; }
    }
}

