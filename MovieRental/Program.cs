using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Users;
using MovieRental.Validators;
using MovieRental.ViewModels.Movies;
using MovieRental.ViewModels.Account;
using MovieRental.ViewModels.Genres;
using MovieRental.ViewModels.People;
using MovieRental.ViewModels.MovieCredits;
using Serilog;
using MovieRental.Services;
using MovieRental.Middleware;


// Configurar Serilog antes de construir el host
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting MovieRental application...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configurar Serilog desde appsettings
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Entity Framework Core con PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ASP.NET Core Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configuración de contraseña
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Configuración de bloqueo
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // Configuración de usuario
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Configuración de cookies
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

    // MVC con Views
    builder.Services.AddControllersWithViews();

    // FluentValidation - Nueva API
    builder.Services.AddFluentValidationAutoValidation(config =>
    {
        config.DisableDataAnnotationsValidation = false; // Mantener DataAnnotations
    })
    .AddFluentValidationClientsideAdapters();

    // Registrar validadores
    builder.Services.AddScoped<IValidator<MovieFormViewModel>, MovieFormValidator>();
    builder.Services.AddScoped<IValidator<RegisterViewModel>, RegisterValidator>();
    builder.Services.AddScoped<IValidator<LoginViewModel>, LoginValidator>();
    builder.Services.AddScoped<IValidator<GenreViewModel>, GenreValidator>();
    builder.Services.AddScoped<IValidator<PersonFormViewModel>, PersonFormValidator>();
    builder.Services.AddScoped<IValidator<MovieCastFormViewModel>, MovieCastValidator>();
    builder.Services.AddScoped<IValidator<MovieCrewFormViewModel>, MovieCrewValidator>();

    // Registrar servicios personalizados
    builder.Services.AddSingleton<ILoggerService, LoggerService>();

    var app = builder.Build();

    // Configurar Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            }
        };
    });

    // Middleware de logging de excepciones
    app.UseExceptionLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

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

    Log.Information("MovieRental application started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}