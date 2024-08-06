using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public MembersController(ApplicationContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // GET: api/Members
        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [Authorize(Roles = "Worker")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }


        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(string id, Member member, string? currentPassword = null)
        {
            ApplicationUser applicationUser = _userManager.FindByIdAsync(id).Result;

            if (id != member.Id)
            {
                return BadRequest();
            }

            applicationUser.Address = member.ApplicationUser!.Address;
            applicationUser.BirthDate = member.ApplicationUser!.BirthDate;
            applicationUser.Email = member.ApplicationUser!.Email;
            applicationUser.Name = member.ApplicationUser!.Name;
            applicationUser.MiddleName = member.ApplicationUser!.MiddleName;
            applicationUser.FamilyName = member.ApplicationUser!.FamilyName;
            applicationUser.Gender = member.ApplicationUser!.Gender;
            applicationUser.UserName = member.ApplicationUser!.UserName;
            applicationUser.PhoneNumber = member.ApplicationUser!.PhoneNumber;


            _userManager.UpdateAsync(applicationUser).Wait();
            if (currentPassword != null)
            {
                _userManager.ChangePasswordAsync(applicationUser, currentPassword, applicationUser.Password).Wait();
            }
            member.ApplicationUser = null;

            _context.Entry(member).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
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

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            if (_context.Members == null)
            {
                return Problem("Entity set 'ApplicationContext.Members'  is null.");
            }
            _userManager.CreateAsync(member.ApplicationUser!, member.ApplicationUser!.Password).Wait();
            _userManager.AddToRoleAsync(member.ApplicationUser, "Member").Wait();

            member.Id = member.ApplicationUser!.Id;
            member.ApplicationUser = null;

            _context.Members.Add(member);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MemberExists(member.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetMember", new { id = member.Id }, member);
        }

        // DELETE: api/Members/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(string id)
        {
            if (_context.Members == null)
            {
                return NotFound();
            }
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPost("Login")]
        public ActionResult Login(string userName, string password)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;
            Microsoft.AspNetCore.Identity.SignInResult signInResult;

            if (applicationUser != null)
            {
                signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
                if (signInResult.Succeeded == true)
                {
                    return Ok();
                }
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet("Logout")]
        public ActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost("ForgetPassword")]
        public ActionResult<string> ForgetPassword(string userName)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;

            string token = _userManager.GeneratePasswordResetTokenAsync(applicationUser).Result;
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage("abc@abc", applicationUser.Email, "Şifre sıfırlama", token);
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient("http://smtp.domain.com");
            smtpClient.Send(mailMessage);
            return token;
        }
        [HttpPost("ResetPassword")]
        public ActionResult ResetPassword(string userName, string token, string newPassword)
        {
            ApplicationUser applicationUser = _userManager.FindByNameAsync(userName).Result;

            _userManager.ResetPasswordAsync(applicationUser, token, newPassword).Wait();

            return Ok();
        }
        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(GoogleAuthModel googleAuthModel)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleAuthModel.idToken, settings);
                return payload;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating Google token: {ex.Message}");
                return null;
            }
        }




        [HttpPost("GoogleRegister")]
        public async Task<IActionResult> GoogleRegister([FromBody] GoogleAuthModel googleAuthModel)
        {
            var payload = await VerifyGoogleToken(googleAuthModel);
            if (payload == null)
            {
                return BadRequest("Invalid Google token.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user != null)
            {
                return BadRequest("User already exists.");
            }

            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("User registration failed.");
            }

            return Ok("User registered successfully.");
        }

        private bool MemberExists(string id)
        {
            return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
