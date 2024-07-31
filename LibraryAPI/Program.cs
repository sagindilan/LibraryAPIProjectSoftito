using LibraryAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Controllers;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace LibraryAPI;

public class Program
{
    public static void Main(string[] args)
    {
        ApplicationContext _context;
        RoleManager<IdentityRole> _roleManager;
        UserManager<ApplicationUser> _userManager;
        ApplicationUser applicationUser;
        IdentityRole identityRole;
        Language language;


        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationContext")));
        builder.Services.AddIdentity<ApplicationUser,IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
        // JWT BURADAN
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
            };
        }); // BURAYA KADAR
        builder.Services.AddAuthorization(options => options.AddPolicy("Makale", policy => policy.RequireClaim("Category", "Makale")));
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        // addSwaggerGen içi boş parantezdi JWT yi içinde göstersin diye doldurduk.
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT", // Autorize olurken token ın başına Bearer yazmamızı istiyordu bunu yazınca gerekmedi.
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                            {
                                {
                                    new OpenApiSecurityScheme
                                    {
                                        Reference = new OpenApiReference
                                        {
                                            Type = ReferenceType.SecurityScheme,
                                            Id = "Bearer"
                                        }
                                    },
                                    new string[] { }
                                }
                            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication(); // JWT
        app.UseAuthorization();


        app.MapControllers();
        
        // DATABASE E PROGRAM CS ÜZER�NDEN MÜDAHALE EDİP ADMİN ROLÜ TANIMLADIK 
        _context = app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationContext>();
        _roleManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        _userManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        _context.Database.Migrate();

        if (_roleManager.FindByNameAsync("Admin").Result == null) // HER AÇILIŞTA YENİDEN ROLE VERMESİN DİYE IF İLE ÖNCE ADMİN VAR MI YOK MU KONTROL EDİLİR YOK İSE EKLENİR
                                                                   // PROGRAM CS HER AÇILIŞTA ÇALIŞIR VE IF KOYMAZ İSEK SÜREKLİ ADMİN KAYDEDER
        {
            identityRole = new IdentityRole("Admin");
            _roleManager.CreateAsync(identityRole).Wait();
        }
        if (_userManager.FindByNameAsync("Admin").Result == null)
        {
            applicationUser = new ApplicationUser();
            applicationUser.UserName = "Admin";
            _userManager.CreateAsync(applicationUser, "Admin123!").Wait();
            _userManager.AddToRoleAsync(applicationUser, "Admin").Wait();
        }
        if (_context.Languages!.Find("tur") == null) //MODELDE YAPTIĞIMIZ LANGUAGE E DATABASE ÜZERİNDEN KAYIT EKLEME EĞER HER ÜLKEYE BU ŞEKİLDE TANIMLAR İSEK LANGUAGE E CONTROLLER YAPMAK GEREKMEZ
        {
            language = new Language();
            language.Code = "tur";
            language.Name = "T�rk�e";
            _context.Languages!.Add(language);
            _context.SaveChanges();
        }

        app.Run();
    }
}

