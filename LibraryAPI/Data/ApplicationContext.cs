using System;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace LibraryAPI.Data
{
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<Location>? Locations { get; set; }
        public DbSet<Language>? Languages { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<SubCategory>? SubCategories { get; set; }
        public DbSet<Publisher>? Publishers { get; set; }
        public DbSet<Author>? Authors { get; set; }
        public DbSet<Book>? Books { get; set; }
        public DbSet<AuthorBook>? AuthorBook { get; set; }
        public DbSet<Member>? Members { get; set; }
        public DbSet<Employee>? Employees { get; set; }
        public DbSet<PurchasedBook>? PurchasedBooks { get; set; }
        public DbSet<BookCopy>? BookCopies { get; set; }
        public DbSet<Department>? Departments { get; set; }
        public DbSet<BookLanguage>? BookLanguage{ get; set; }
        public DbSet<BookSubCategory>? BookSubCategory { get; set; }
        public DbSet<DonatedBook>? DonatedBooks { get; set; }
        public DbSet<Reservation>? Reservations { get; set; }
        public DbSet<Rating>? Ratings { get; set; }
        public DbSet<Loan>? Loans { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AuthorBook>().HasKey(a => new { a.AuthorsId, a.BooksId });
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BookSubCategory>().HasKey(a => new { a.BookId, a.SubCategoryId });
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BookLanguage>().HasKey(a => new { a.BookId, a.LanguageId });


            

            modelBuilder.Entity<PurchasedBook>()
               .HasOne(k => k.Publisher)
               .WithMany(n => n.PurchasedBook)
               .HasForeignKey(k => k.PublisherId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
              .HasOne(l => l.Member)
              .WithMany(m => m.Loans)
              .HasForeignKey(l => l.MemberId)
              .OnDelete(DeleteBehavior.Restrict);

        }

        public DbSet<LibraryAPI.Models.Rating>? Rating { get; set; }

    }
}

