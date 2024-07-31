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
    public class DonatedBooksController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public DonatedBooksController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/DonatedBooks
        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonatedBook>>> GetDonatedBooks()
        {
          if (_context.DonatedBooks == null)
          {
              return NotFound();
          }
            return await _context.DonatedBooks.ToListAsync();
        }

        // GET: api/DonatedBooks/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DonatedBook>> GetDonatedBook(int id)
        {
          if (_context.DonatedBooks == null)
          {
              return NotFound();
          }
            var donatedBook = await _context.DonatedBooks.FindAsync(id);

            if (donatedBook == null)
            {
                return NotFound();
            }

            return donatedBook;
        }

        // PUT: api/DonatedBooks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonatedBook(int id, DonatedBook donatedBook)
        {
            if (id != donatedBook.Id)
            {
                return BadRequest();
            }

            _context.Entry(donatedBook).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DonatedBookExists(id))
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

        // POST: api/DonatedBooks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPost]
        public async Task<ActionResult<DonatedBook>> PostDonatedBook(DonatedBook donatedBook)
        {
          if (_context.DonatedBooks == null)
          {
              return Problem("Entity set 'ApplicationContext.DonatedBooks'  is null.");
          }
            _context.DonatedBooks.Add(donatedBook);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDonatedBook", new { id = donatedBook.Id }, donatedBook);
        }

        // DELETE: api/DonatedBooks/5
        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonatedBook(int id)
        {
            if (_context.DonatedBooks == null)
            {
                return NotFound();
            }
            var donatedBook = await _context.DonatedBooks.FindAsync(id);
            if (donatedBook == null)
            {
                return NotFound();
            }

            _context.DonatedBooks.Remove(donatedBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DonatedBookExists(int id)
        {
            return (_context.DonatedBooks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
