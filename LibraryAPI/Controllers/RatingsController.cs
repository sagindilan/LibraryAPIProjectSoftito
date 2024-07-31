using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public RatingsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Ratings
        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rating>>> GetRating()
        {
          if (_context.Rating == null)
          {
              return NotFound();
          }
            return await _context.Rating.ToListAsync();
        }

        // GET: api/Ratings/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Rating>> GetRating(int id)
        {
          if (_context.Rating == null)
          {
              return NotFound();
          }
            var rating = await _context.Rating.FindAsync(id);

            if (rating == null)
            {
                return NotFound();
            }

            return rating;
        }

        // PUT: api/Ratings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRating(int id, Rating rating)
        {
            if (id != rating.Id)
            {
                return BadRequest();
            }

            _context.Entry(rating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Ratings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Rating>> PostRating(Rating rating)
        {
            if (_context.Ratings == null)
            {
                return Problem("Entity set 'ApplicationContext.Ratings' is null.");
            }

            if (rating.Score < 1 || rating.Score > 5)
            {
                return BadRequest("Puan 1 ile 5 arasında olmalıdır.");
            }

            var borrowingHistories = _context.BorrowingHistories!.FirstOrDefault(l => l.BookCopyId == rating.BookCopyId && l.MemberId == rating.MemberId && l.ReturnDate != null);
            if (borrowingHistories == null)
            {
                return BadRequest("Bu kitabı ödünç alıp iade etmediniz.");
            }

            var existingRating = _context.Ratings!.FirstOrDefault(r => r.BookCopyId == rating.BookCopyId && r.MemberId == rating.MemberId);
            if (existingRating != null)
            {
                return BadRequest("Bu kitabı zaten değerlendirdiniz.");
            }

            var bookCopy = _context.BookCopies!.FirstOrDefault(bc => bc.Id == rating.BookCopyId);
            if (bookCopy == null)
            {
                return NotFound("Kitap kopyası bulunamadı.");
            }

            // Puanı ekle ve ortalamayı güncelle
            bookCopy.VoteCount++; // Kitap kopyasına yapılan toplam oy sayısını artır.
            bookCopy.VoteSum += rating.Score; // Kitap kopyasının toplam puanına yeni puanı ekle.
            bookCopy.Rating = (double)bookCopy.VoteSum / bookCopy.VoteCount; // Yeni ortalama puanı hesapla.

            _context.BookCopies!.Update(bookCopy);
            _context.Ratings!.Add(rating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRating", new { id = rating.Id }, rating);
        }



        // DELETE: api/Ratings/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            if (_context.Rating == null)
            {
                return NotFound();
            }
            var rating = await _context.Rating.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            _context.Rating.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RatingExists(int id)
        {
            return (_context.Rating?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
