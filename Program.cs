using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using MovieRental.Validators;
using MovieRental.ViewModels.Movies;
using MovieRental.ViewModels.Account;
using MovieRental.ViewModels.Genres;
using MovieRental.ViewModels.People;
using MovieRental.ViewModels.MovieCredits;

var builder = WebApplication.CreateBuilder(args);

// 1. DB Context con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configuración de Password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Configuración de Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configuración de User
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Configuración de Cookies para Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// 4. Servicios MVC
builder.Services.AddControllersWithViews();

// FluentValidation - usando la nueva API
builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = false; // Mantener DataAnnotations activas
})
.AddFluentValidationClientsideAdapters(); // Habilitar validación del lado del cliente

// Registrar validadores
builder.Services.AddScoped<IValidator<MovieFormViewModel>, MovieFormValidator>();
builder.Services.AddScoped<IValidator<RegisterViewModel>, RegisterValidator>();
builder.Services.AddScoped<IValidator<LoginViewModel>, LoginValidator>();
builder.Services.AddScoped<IValidator<GenreViewModel>, GenreValidator>();
builder.Services.AddScoped<IValidator<PersonFormViewModel>, PersonFormValidator>();
builder.Services.AddScoped<IValidator<MovieCastFormViewModel>, MovieCastValidator>();
builder.Services.AddScoped<IValidator<MovieCrewFormViewModel>, MovieCrewValidator>();

var app = builder.Build();

// ========================================
// Configuración del Pipeline HTTP
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication debe ir ANTES de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed de datos iniciales
await DbSeeder.SeedAsync(app.Services);

// Seed de películas adicionales para pruebas de paginación
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await AdditionalSeeder.SeedMoreMoviesAsync(context);
}

app.Run();




