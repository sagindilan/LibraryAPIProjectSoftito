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
    public class PurchasedBooksController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public PurchasedBooksController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/PurchasedBooks
        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchasedBook>>> GetPurchasedBooks()
        {
          if (_context.PurchasedBooks == null)
          {
              return NotFound();
          }
            return await _context.PurchasedBooks.ToListAsync();
        }

        // GET: api/PurchasedBooks/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PurchasedBook>> GetPurchasedBook(int id)
        {
          if (_context.PurchasedBooks == null)
          {
              return NotFound();
          }
            var purchasedBook = await _context.PurchasedBooks.FindAsync(id);

            if (purchasedBook == null)
            {
                return NotFound();
            }

            return purchasedBook;
        }

        // PUT: api/PurchasedBooks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPurchasedBook(int id, PurchasedBook purchasedBook)
        {
            if (id != purchasedBook.Id)
            {
                return BadRequest();
            }

            _context.Entry(purchasedBook).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchasedBookExists(id))
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

        // POST: api/PurchasedBooks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPost]
        public async Task<ActionResult<PurchasedBook>> PostPurchasedBook(PurchasedBook purchasedBook)
        {
          if (_context.PurchasedBooks == null)
          {
              return Problem("Entity set 'ApplicationContext.PurchasedBooks'  is null.");
          }
            _context.PurchasedBooks.Add(purchasedBook);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPurchasedBook", new { id = purchasedBook.Id }, purchasedBook);
        }

        // DELETE: api/PurchasedBooks/5
        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchasedBook(int id)
        {
            if (_context.PurchasedBooks == null)
            {
                return NotFound();
            }
            var purchasedBook = await _context.PurchasedBooks.FindAsync(id);
            if (purchasedBook == null)
            {
                return NotFound();
            }

            _context.PurchasedBooks.Remove(purchasedBook);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PurchasedBookExists(int id)
        {
            return (_context.PurchasedBooks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
