using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRental.Models.Movies;
using MovieRental.Models.Users;

namespace MovieRental.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Asegurar que la BD está creada y migrada
        await context.Database.MigrateAsync();

        // Seed en orden
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedGenresAsync(context);
        await SeedPeopleAsync(context);
        await SeedMoviesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin user
        if (await userManager.FindByEmailAsync("admin@movierental.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@movierental.com",
                Email = "admin@movierental.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // Customer user
        if (await userManager.FindByEmailAsync("customer@example.com") == null)
        {
            var customer = new ApplicationUser
            {
                UserName = "customer@example.com",
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(customer, "Customer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customer, "Customer");
            }
        }
    }

    private static async Task SeedGenresAsync(ApplicationDbContext context)
    {
        if (await context.Genres.AnyAsync()) return;

        var genres = new List<Genre>
        {
            new() { Name = "Action" },
            new() { Name = "Adventure" },
            new() { Name = "Comedy" },
            new() { Name = "Drama" },
            new() { Name = "Horror" },
            new() { Name = "Science Fiction" },
            new() { Name = "Thriller" },
            new() { Name = "Romance" },
            new() { Name = "Animation" },
            new() { Name = "Documentary" },
            new() { Name = "Fantasy" },
            new() { Name = "Crime" }
        };

        await context.Genres.AddRangeAsync(genres);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPeopleAsync(ApplicationDbContext context)
    {
        if (await context.People.AnyAsync()) return;

        var people = new List<Person>
        {
            new()
            {
                FullName = "Christopher Nolan",
                BirthDate = DateTime.SpecifyKind(new DateTime(1970, 7, 30), DateTimeKind.Utc),
                Bio = "British-American filmmaker known for his cerebral, often nonlinear storytelling."
            },
            new()
            {
                FullName = "Leonardo DiCaprio",
                BirthDate = DateTime.SpecifyKind(new DateTime(1974, 11, 11), DateTimeKind.Utc),
                Bio = "American actor and film producer known for his work in biopics and period films."
            },
            new()
            {
                FullName = "Joseph Gordon-Levitt",
                BirthDate = DateTime.SpecifyKind(new DateTime(1981, 2, 17), DateTimeKind.Utc),
                Bio = "American actor and filmmaker who has received various accolades."
            },
            new()
            {
                FullName = "Ellen Page",
                BirthDate = DateTime.SpecifyKind(new DateTime(1987, 2, 21), DateTimeKind.Utc),
                Bio = "Canadian actor and producer."
            },
            new()
            {
                FullName = "Tom Hardy",
                BirthDate = DateTime.SpecifyKind(new DateTime(1977, 9, 15), DateTimeKind.Utc),
                Bio = "English actor and producer."
            },
            new()
            {
                FullName = "Quentin Tarantino",
                BirthDate = DateTime.SpecifyKind(new DateTime(1963, 3, 27), DateTimeKind.Utc),
                Bio = "American filmmaker and screenwriter known for his stylized films."
            },
            new()
            {
                FullName = "Samuel L. Jackson",
                BirthDate = DateTime.SpecifyKind(new DateTime(1948, 12, 21), DateTimeKind.Utc),
                Bio = "American actor and producer, one of the most widely recognized actors of his generation."
            },
            new()
            {
                FullName = "Uma Thurman",
                BirthDate = DateTime.SpecifyKind(new DateTime(1970, 4, 29), DateTimeKind.Utc),
                Bio = "American actress and model."
            },
            new()
            {
                FullName = "John Travolta",
                BirthDate = DateTime.SpecifyKind(new DateTime(1954, 2, 18), DateTimeKind.Utc),
                Bio = "American actor, singer, and dancer."
            },
            new()
            {
                FullName = "Frank Darabont",
                BirthDate = DateTime.SpecifyKind(new DateTime(1959, 1, 28), DateTimeKind.Utc),
                Bio = "Hungarian-American film director, screenwriter and producer."
            },
            new()
            {
                FullName = "Tim Robbins",
                BirthDate = DateTime.SpecifyKind(new DateTime(1958, 10, 16), DateTimeKind.Utc),
                Bio = "American actor, filmmaker, and activist."
            },
            new()
            {
                FullName = "Morgan Freeman",
                BirthDate = DateTime.SpecifyKind(new DateTime(1937, 6, 1), DateTimeKind.Utc),
                Bio = "American actor, director, and narrator."
            }
        };

        await context.People.AddRangeAsync(people);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMoviesAsync(ApplicationDbContext context)
    {
        if (await context.Movies.AnyAsync()) return;

        // Obtener referencias
        var genres = await context.Genres.ToListAsync();
        var people = await context.People.ToListAsync();

        var movies = new List<Movie>
        {
            new()
            {
                Title = "Inception",
                OriginalTitle = "Inception",
                Synopsis = "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea into the mind of a C.E.O.",
                ReleaseYear = 2010,
                DurationMinutes = 148,
                Language = "English",
                RentalPrice = 4.99m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Title = "Pulp Fiction",
                OriginalTitle = "Pulp Fiction",
                Synopsis = "The lives of two mob hitmen, a boxer, a gangster and his wife, and a pair of diner bandits intertwine in four tales of violence and redemption.",
                ReleaseYear = 1994,
                DurationMinutes = 154,
                Language = "English",
                RentalPrice = 3.99m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Title = "The Shawshank Redemption",
                OriginalTitle = "The Shawshank Redemption",
                Synopsis = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                ReleaseYear = 1994,
                DurationMinutes = 142,
                Language = "English",
                RentalPrice = 3.99m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Title = "The Dark Knight",
                OriginalTitle = "The Dark Knight",
                Synopsis = "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests of his ability to fight injustice.",
                ReleaseYear = 2008,
                DurationMinutes = 152,
                Language = "English",
                RentalPrice = 4.99m,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync();

        // Reload movies with IDs
        movies = await context.Movies.ToListAsync();

        // Seed MovieGenres
        var movieGenres = new List<MovieGenre>
        {
            // Inception: Action, Science Fiction, Thriller
            new() { MovieId = movies[0].MovieId, GenreId = genres.First(g => g.Name == "Action").GenreId },
            new() { MovieId = movies[0].MovieId, GenreId = genres.First(g => g.Name == "Science Fiction").GenreId },
            new() { MovieId = movies[0].MovieId, GenreId = genres.First(g => g.Name == "Thriller").GenreId },

            // Pulp Fiction: Crime, Drama
            new() { MovieId = movies[1].MovieId, GenreId = genres.First(g => g.Name == "Crime").GenreId },
            new() { MovieId = movies[1].MovieId, GenreId = genres.First(g => g.Name == "Drama").GenreId },

            // Shawshank: Drama
            new() { MovieId = movies[2].MovieId, GenreId = genres.First(g => g.Name == "Drama").GenreId },

            // Dark Knight: Action, Crime, Drama, Thriller
            new() { MovieId = movies[3].MovieId, GenreId = genres.First(g => g.Name == "Action").GenreId },
            new() { MovieId = movies[3].MovieId, GenreId = genres.First(g => g.Name == "Crime").GenreId },
            new() { MovieId = movies[3].MovieId, GenreId = genres.First(g => g.Name == "Drama").GenreId },
            new() { MovieId = movies[3].MovieId, GenreId = genres.First(g => g.Name == "Thriller").GenreId }
        };

        await context.MovieGenres.AddRangeAsync(movieGenres);

        // Seed MovieCast
        var movieCasts = new List<MovieCast>
        {
            // Inception cast
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Leonardo DiCaprio").PersonId,
                CharacterName = "Dom Cobb",
                CastOrder = 1
            },
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Joseph Gordon-Levitt").PersonId,
                CharacterName = "Arthur",
                CastOrder = 2
            },
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Ellen Page").PersonId,
                CharacterName = "Ariadne",
                CastOrder = 3
            },
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Tom Hardy").PersonId,
                CharacterName = "Eames",
                CastOrder = 4
            },

            // Pulp Fiction cast
            new() 
            { 
                MovieId = movies[1].MovieId, 
                PersonId = people.First(p => p.FullName == "Samuel L. Jackson").PersonId,
                CharacterName = "Jules Winnfield",
                CastOrder = 1
            },
            new() 
            { 
                MovieId = movies[1].MovieId, 
                PersonId = people.First(p => p.FullName == "John Travolta").PersonId,
                CharacterName = "Vincent Vega",
                CastOrder = 2
            },
            new() 
            { 
                MovieId = movies[1].MovieId, 
                PersonId = people.First(p => p.FullName == "Uma Thurman").PersonId,
                CharacterName = "Mia Wallace",
                CastOrder = 3
            },

            // Shawshank cast
            new() 
            { 
                MovieId = movies[2].MovieId, 
                PersonId = people.First(p => p.FullName == "Tim Robbins").PersonId,
                CharacterName = "Andy Dufresne",
                CastOrder = 1
            },
            new() 
            { 
                MovieId = movies[2].MovieId, 
                PersonId = people.First(p => p.FullName == "Morgan Freeman").PersonId,
                CharacterName = "Ellis Boyd 'Red' Redding",
                CastOrder = 2
            }
        };

        await context.MovieCasts.AddRangeAsync(movieCasts);

        // Seed MovieCrew
        var movieCrews = new List<MovieCrew>
        {
            // Inception crew
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Christopher Nolan").PersonId,
                Role = "Director"
            },
            new() 
            { 
                MovieId = movies[0].MovieId, 
                PersonId = people.First(p => p.FullName == "Christopher Nolan").PersonId,
                Role = "Writer"
            },

            // Pulp Fiction crew
            new() 
            { 
                MovieId = movies[1].MovieId, 
                PersonId = people.First(p => p.FullName == "Quentin Tarantino").PersonId,
                Role = "Director"
            },
            new() 
            { 
                MovieId = movies[1].MovieId, 
                PersonId = people.First(p => p.FullName == "Quentin Tarantino").PersonId,
                Role = "Writer"
            },

            // Shawshank crew
            new() 
            { 
                MovieId = movies[2].MovieId, 
                PersonId = people.First(p => p.FullName == "Frank Darabont").PersonId,
                Role = "Director"
            },
            new() 
            { 
                MovieId = movies[2].MovieId, 
                PersonId = people.First(p => p.FullName == "Frank Darabont").PersonId,
                Role = "Writer"
            },

            // Dark Knight crew
            new() 
            { 
                MovieId = movies[3].MovieId, 
                PersonId = people.First(p => p.FullName == "Christopher Nolan").PersonId,
                Role = "Director"
            }
        };

        await context.MovieCrews.AddRangeAsync(movieCrews);

        await context.SaveChangesAsync();
    }
}