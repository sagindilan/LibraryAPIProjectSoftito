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
    public class LoansController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoansController(ApplicationContext context, UserManager<ApplicationUser> userManger)
        {
            _context = context;
            _userManager = userManger;
        }

        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            if (_context.Loans == null)
            {
                return NotFound();
            }
            return await _context.Loans.ToListAsync();
        }

        // GET: api/Loans/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            if (_context.Loans == null)
            {
                return NotFound();
            }
            var loans = await _context.Loans.FindAsync(id);

            if (loans == null)
            {
                return NotFound();
            }

            return loans;
        }

        // PUT: api/Loans/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Worker")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoan(int id, Loan loan)
        {
            if (id != loan.Id)
            {
                return BadRequest();
            }

            var existingloan = await _context.Loans!.FindAsync(id);
            if (existingloan == null)
            {
                return NotFound();
            }


            // Geç iade ve hasar cezasını hesapla
            int penaltyAmount = 0;

            if (loan.ReturnDate.HasValue && loan.ReturnDate.Value > loan.DueDate)
            {
                var daysLate = (loan.ReturnDate.Value - loan.DueDate).Days;
                penaltyAmount += daysLate * Loan.PenaltyPerDay;
            }

            if (loan.IsDamaged)
            {
                penaltyAmount += Loan.DamagePenalty;
            }

            existingloan.PenaltyAmount = penaltyAmount;
            existingloan.ReturnDate = loan.ReturnDate;
            existingloan.IsDamaged = loan.IsDamaged;

            _context.Entry(existingloan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Üyenin ceza tutarını güncelle
                if (penaltyAmount > 0)
                {
                    var member = await _context.Members!.FindAsync(existingloan.MemberId);
                    if (member != null)
                    {
                        member.TotalPenalty += penaltyAmount; // Üyenin toplam cezasına ekleme yapın
                        _context.Entry(member).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                // Kitap kopyasının durumu güncelleniyor
                var bookCopy = await _context.BookCopies!.FindAsync(existingloan.BookCopyId);
                if (bookCopy != null)
                {
                    bookCopy.IsAvailable = true; // Kitap iade edildiğinde tekrar rafta görünsün
                    _context.Entry(bookCopy).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(id))
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
        public async Task<ActionResult<Loan>> PostLoan([FromBody] Loan loan, string userName)
        {
            if (_context.Loans == null)
            {
                return Problem("Entity set 'ApplicationContext.Loans' is null.");
            }

            var user = await _userManager.FindByNameAsync(userName); // Buradan Kitap Ödünç alırken MemberId yi girmek yerine username i alıp username den MemberId yi çekiyoruz.
            if (user == null)
            {
                return NotFound("User not found.");
            }
            loan.MemberId = user.Id; // Buraya



            // Kitap kopyasını al
            //kitap kopyasıyla loan ilişkisi yok en son sorucam onu sana sıra gelicek

            var bookCopy = await _context.BookCopies.FindAsync(loan.BookCopyId);
            if (bookCopy == null)
            {
                return NotFound("Book copy not found.");
            }

            // Kitap mevcut mu ve ödünç verilebilir mi kontrol et
            if (!bookCopy.IsAvailable)
            {
                return BadRequest("Book copy is not available for loaning.");
            }

            // Ödünç verme işlemini kaydet
            loan.BorrowDate = DateTime.Now;
            loan.DueDate = DateTime.Now.AddDays(14); // Ödünç verme süresi (14 gün)

            // Kitap ödünç verildi, durumunu güncelle
            bookCopy.IsAvailable = false;

            // Veritabanına kaydet
            _context.Loans.Add(loan);
            _context.Entry(bookCopy).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLoan", new { id = loan.Id }, loan);
        }


        [Authorize(Roles = "Worker")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            if (_context.Loans == null)
            {
                return NotFound();
            }
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LoanExists(int id)
        {
            return (_context.Loans?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}