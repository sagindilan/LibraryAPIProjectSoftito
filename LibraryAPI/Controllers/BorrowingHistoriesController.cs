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
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BorrowingHistoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BorrowingHistoriesController(ApplicationContext context,UserManager<ApplicationUser> userManger)
        {
            _context = context;
            _userManager = userManger;
        }

        // GET: api/BorrowingHistories
        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BorrowingHistory>>> GetBorrowingHistories()
        {
          if (_context.BorrowingHistories == null)
          {
              return NotFound();
          }
            return await _context.BorrowingHistories.ToListAsync();
        }

        // GET: api/BorrowingHistories/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BorrowingHistory>> GetBorrowingHistory(int id)
        {
          if (_context.BorrowingHistories == null)
          {
              return NotFound();
          }
            var borrowingHistory = await _context.BorrowingHistories.FindAsync(id);

            if (borrowingHistory == null)
            {
                return NotFound();
            }

            return borrowingHistory;
        }

        // PUT: api/BorrowingHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBorrowingHistory(int id, BorrowingHistory borrowingHistory)
        {
            if (id != borrowingHistory.Id)
            {
                return BadRequest();
            }

            var existingBorrowingHistory = await _context.BorrowingHistories!.FindAsync(id);
            if (existingBorrowingHistory == null)
            {
                return NotFound();
            }

            
            // Geç iade ve hasar cezasını hesapla
            int penaltyAmount = 0;

            if (borrowingHistory.ReturnDate.HasValue && borrowingHistory.ReturnDate.Value > borrowingHistory.DueDate)
            {
                var daysLate = (borrowingHistory.ReturnDate.Value - borrowingHistory.DueDate).Days;
                penaltyAmount += daysLate * BorrowingHistory.PenaltyPerDay;
            }

            if (borrowingHistory.IsDamaged)
            {
                penaltyAmount += BorrowingHistory.DamagePenalty;
            }
           
            existingBorrowingHistory.PenaltyAmount = penaltyAmount;
            existingBorrowingHistory.ReturnDate = borrowingHistory.ReturnDate;
            existingBorrowingHistory.IsDamaged = borrowingHistory.IsDamaged;

            _context.Entry(existingBorrowingHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Üyenin ceza tutarını güncelle
                if (penaltyAmount > 0)
                {
                    var member = await _context.Members!.FindAsync(existingBorrowingHistory.MemberId);
                    if (member != null)
                    {
                        member.TotalPenalty += penaltyAmount; // Üyenin toplam cezasına ekleme yapın
                        _context.Entry(member).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                // Kitap kopyasının durumu güncelleniyor
                var bookCopy = await _context.BookCopies!.FindAsync(existingBorrowingHistory.BookCopyId);
                if (bookCopy != null)
                {
                    bookCopy.IsAvailable = true; // Kitap iade edildiğinde tekrar rafta görünsün
                    _context.Entry(bookCopy).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BorrowingHistoryExists(id))
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

        [Authorize(Roles = "Worker")]
        [HttpPost]
        public async Task<ActionResult<BorrowingHistory>> PostBorrowingHistory([FromBody] BorrowingHistory borrowingHistory, string userName)
        {
            if (_context.BorrowingHistories == null)
            {
                return Problem("Entity set 'ApplicationContext.BorrowingHistories' is null.");
            }

            var user = await _userManager.FindByNameAsync(userName); // Buradan Kitap Ödünç alırken MemberId yi girmek yerine username i alıp username den MemberId yi çekiyoruz.
            if (user == null)
            {
                return NotFound("User not found.");
            }
            borrowingHistory.MemberId = user.Id; // Buraya



            // Kitap kopyasını al
            var bookCopy = await _context.BookCopies.FindAsync(borrowingHistory.BookCopyId);
            if (bookCopy == null)
            {
                return NotFound("Book copy not found.");
            }

            // Kitap mevcut mu ve ödünç verilebilir mi kontrol et
            if (!bookCopy.IsAvailable)
            {
                return BadRequest("Book copy is not available for borrowing.");
            }

            // Ödünç verme işlemini kaydet
            borrowingHistory.BorrowDate = DateTime.Now;
            borrowingHistory.DueDate = DateTime.Now.AddDays(14); // Ödünç verme süresi (14 gün)

            // Kitap ödünç verildi, durumunu güncelle
            bookCopy.IsAvailable = false;

            // Veritabanına kaydet
            _context.BorrowingHistories.Add(borrowingHistory);
            _context.Entry(bookCopy).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBorrowingHistory", new { id = borrowingHistory.Id }, borrowingHistory);
        }

        //[Authorize(Roles = "Worker")]
        //[HttpPost]
        //public async Task<ActionResult<BorrowingHistory>> PostBorrowingHistory(BorrowingHistory borrowingHistory, string userName)
        //{
        //    if (_context.BorrowingHistories == null)
        //    {
        //        return Problem("Entity set 'ApplicationContext.BorrowingHistories'  is null.");
        //    }

        //    // Kullanıcıyı al
        //    var user = await _userManager.FindByNameAsync(userName);
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }
        //    borrowingHistory.MemberId = user.Id;

        //    // Kitap kopyasını al
        //    var bookCopy = await _context.BookCopies.FindAsync(bookCopyId);
        //    if (bookCopy == null)
        //    {
        //        return NotFound("Book copy not found.");
        //    }

        //    // Kitap mevcut mu ve ödünç verilebilir mi kontrol et
        //    if (!bookCopy.IsAvailable)
        //    {
        //        return BadRequest("Book copy is not available for borrowing.");
        //    }

        //    // Ödünç verme işlemini kaydet
        //    borrowingHistory.BookCopyId = bookCopyId;
        //    borrowingHistory.BorrowDate = DateTime.Now;
        //    borrowingHistory.DueDate = DateTime.Now.AddDays(14); // Ödünç verme süresi (14 gün)

        //    // Kitap ödünç verildi, durumunu güncelle
        //    bookCopy.IsAvailable = false;

        //    // Veritabanına kaydet
        //    _context.BorrowingHistories.Add(borrowingHistory);
        //    _context.Entry(bookCopy).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetBorrowingHistory", new { id = borrowingHistory.Id }, borrowingHistory);
        //}


        //// POST: api/BorrowingHistories
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[Authorize(Roles = "Worker")]
        //[HttpPost]
        //public async Task<ActionResult<BorrowingHistory>> PostBorrowingHistory(BorrowingHistory borrowingHistory, string userName)
        //{
        //    if (_context.BorrowingHistories == null)
        //    {
        //        return Problem("Entity set 'ApplicationContext.BorrowingHistories'  is null.");
        //    }

        //    var user = await _userManager.FindByNameAsync(userName); // Buradan Kitap Ödünç alırken MemberId yi girmek yerine username i alıp username den MemberId yi çekiyoruz.
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }
        //    borrowingHistory.MemberId = user.Id; // Buraya

        //    var bookCopy = await _context.BookCopies.FindAsync(bookCopyId);

        //    if (bookCopy == null)
        //    {
        //        return NotFound("Book copy not found.");
        //    }

        //    // Kitap mevcut mu ve rafta mı kontrol et
        //    if (!bookCopy.Condition)
        //    {
        //        return BadRequest("Book copy is not available for borrowing.");
        //    }


        //    return CreatedAtAction("GetBorrowingHistory", new { id = borrowingHistory.Id }, borrowingHistory);
        //}

        // DELETE: api/BorrowingHistories/5
        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBorrowingHistory(int id)
        {
            if (_context.BorrowingHistories == null)
            {
                return NotFound();
            }
            var borrowingHistory = await _context.BorrowingHistories.FindAsync(id);
            if (borrowingHistory == null)
            {
                return NotFound();
            }

            _context.BorrowingHistories.Remove(borrowingHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BorrowingHistoryExists(int id)
        {
           return (_context.BorrowingHistories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
