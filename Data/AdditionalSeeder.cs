using Microsoft.EntityFrameworkCore;
using MovieRental.Models.Movies;

namespace MovieRental.Data;

public static class AdditionalSeeder
{
    public static async Task SeedMoreMoviesAsync(ApplicationDbContext context)
    {
        // Solo agregar si hay pocas películas
        if (await context.Movies.CountAsync() > 10)
            return;

        var genres = await context.Genres.ToListAsync();
        var actionGenre = genres.FirstOrDefault(g => g.Name == "Action");
        var dramaGenre = genres.FirstOrDefault(g => g.Name == "Drama");
        var scifiGenre = genres.FirstOrDefault(g => g.Name == "Science Fiction");
        var comedyGenre = genres.FirstOrDefault(g => g.Name == "Comedy");
        var thrillerGenre = genres.FirstOrDefault(g => g.Name == "Thriller");
        var horrorGenre = genres.FirstOrDefault(g => g.Name == "Horror");

        var additionalMovies = new List<Movie>
        {
            new() { Title = "The Matrix", OriginalTitle = "The Matrix", ReleaseYear = 1999, DurationMinutes = 136, Language = "English", RentalPrice = 3.99m, Synopsis = "A computer hacker learns about the true nature of reality and his role in the war against its controllers." },
            new() { Title = "Interstellar", OriginalTitle = "Interstellar", ReleaseYear = 2014, DurationMinutes = 169, Language = "English", RentalPrice = 4.99m, Synopsis = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival." },
            new() { Title = "The Godfather", OriginalTitle = "The Godfather", ReleaseYear = 1972, DurationMinutes = 175, Language = "English", RentalPrice = 3.49m, Synopsis = "The aging patriarch of an organized crime dynasty transfers control to his reluctant son." },
            new() { Title = "Fight Club", OriginalTitle = "Fight Club", ReleaseYear = 1999, DurationMinutes = 139, Language = "English", RentalPrice = 3.99m, Synopsis = "An insomniac office worker and a soap salesman build a global organization to help vent male aggression." },
            new() { Title = "Forrest Gump", OriginalTitle = "Forrest Gump", ReleaseYear = 1994, DurationMinutes = 142, Language = "English", RentalPrice = 3.49m, Synopsis = "The presidencies of Kennedy and Johnson, the Vietnam War, and other events unfold from the perspective of an Alabama man." },
            new() { Title = "The Lord of the Rings: The Fellowship", OriginalTitle = "The Lord of the Rings: The Fellowship of the Ring", ReleaseYear = 2001, DurationMinutes = 178, Language = "English", RentalPrice = 4.49m, Synopsis = "A meek Hobbit sets out on a journey to destroy a powerful ring." },
            new() { Title = "Gladiator", OriginalTitle = "Gladiator", ReleaseYear = 2000, DurationMinutes = 155, Language = "English", RentalPrice = 3.99m, Synopsis = "A former Roman General sets out to exact vengeance against the corrupt emperor." },
            new() { Title = "The Prestige", OriginalTitle = "The Prestige", ReleaseYear = 2006, DurationMinutes = 130, Language = "English", RentalPrice = 4.49m, Synopsis = "Two stage magicians engage in competitive one-upmanship in an attempt to create the ultimate stage illusion." },
            new() { Title = "Se7en", OriginalTitle = "Se7en", ReleaseYear = 1995, DurationMinutes = 127, Language = "English", RentalPrice = 3.49m, Synopsis = "Two detectives hunt a serial killer who uses the seven deadly sins as his motives." },
            new() { Title = "The Silence of the Lambs", OriginalTitle = "The Silence of the Lambs", ReleaseYear = 1991, DurationMinutes = 118, Language = "English", RentalPrice = 3.49m, Synopsis = "A young FBI cadet must receive the help of an incarcerated cannibal killer." },
            new() { Title = "Saving Private Ryan", OriginalTitle = "Saving Private Ryan", ReleaseYear = 1998, DurationMinutes = 169, Language = "English", RentalPrice = 3.99m, Synopsis = "Following the Normandy Landings, a group of soldiers go behind enemy lines to retrieve a paratrooper." },
            new() { Title = "The Green Mile", OriginalTitle = "The Green Mile", ReleaseYear = 1999, DurationMinutes = 189, Language = "English", RentalPrice = 3.99m, Synopsis = "The lives of guards on Death Row are affected by one of their charges." },
            new() { Title = "Schindler's List", OriginalTitle = "Schindler's List", ReleaseYear = 1993, DurationMinutes = 195, Language = "English", RentalPrice = 3.49m, Synopsis = "In German-occupied Poland, industrialist Oskar Schindler becomes concerned for his Jewish workforce." },
            new() { Title = "Jurassic Park", OriginalTitle = "Jurassic Park", ReleaseYear = 1993, DurationMinutes = 127, Language = "English", RentalPrice = 3.99m, Synopsis = "A pragmatic paleontologist visiting an almost complete theme park is tasked with protecting a couple of kids." },
            new() { Title = "The Lion King", OriginalTitle = "The Lion King", ReleaseYear = 1994, DurationMinutes = 88, Language = "English", RentalPrice = 3.49m, Synopsis = "Lion prince Simba flees his kingdom only to learn the true meaning of responsibility and bravery." },
            new() { Title = "Back to the Future", OriginalTitle = "Back to the Future", ReleaseYear = 1985, DurationMinutes = 116, Language = "English", RentalPrice = 3.49m, Synopsis = "Marty McFly is accidentally sent 30 years into the past in a time-traveling DeLorean." },
            new() { Title = "Terminator 2", OriginalTitle = "Terminator 2: Judgment Day", ReleaseYear = 1991, DurationMinutes = 137, Language = "English", RentalPrice = 3.99m, Synopsis = "A cyborg protects a boy from a more advanced killing machine sent from the future." },
            new() { Title = "Alien", OriginalTitle = "Alien", ReleaseYear = 1979, DurationMinutes = 117, Language = "English", RentalPrice = 3.49m, Synopsis = "The crew of a commercial spacecraft encounter a deadly lifeform after investigating a mysterious transmission." },
            new() { Title = "Die Hard", OriginalTitle = "Die Hard", ReleaseYear = 1988, DurationMinutes = 132, Language = "English", RentalPrice = 3.49m, Synopsis = "An NYPD officer tries to save his wife and others taken hostage by German terrorists." },
            new() { Title = "Goodfellas", OriginalTitle = "Goodfellas", ReleaseYear = 1990, DurationMinutes = 146, Language = "English", RentalPrice = 3.49m, Synopsis = "The story of Henry Hill and his life in the mob." }
        };

        await context.Movies.AddRangeAsync(additionalMovies);
        await context.SaveChangesAsync();

        // Reload movies to get IDs
        var movies = await context.Movies.ToListAsync();

        // Add genres to new movies
        var movieGenres = new List<MovieGenre>();

        foreach (var movie in movies)
        {
            // Skip if already has genres
            if (await context.MovieGenres.AnyAsync(mg => mg.MovieId == movie.MovieId))
                continue;

            // Assign genres based on movie characteristics
            if (movie.Title.Contains("Matrix") || movie.Title.Contains("Terminator") || movie.Title.Contains("Alien"))
            {
                if (scifiGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = scifiGenre.GenreId });
                if (actionGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = actionGenre.GenreId });
            }
            else if (movie.Title.Contains("Godfather") || movie.Title.Contains("Goodfellas"))
            {
                if (dramaGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = dramaGenre.GenreId });
            }
            else if (movie.Title.Contains("Se7en") || movie.Title.Contains("Silence"))
            {
                if (thrillerGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = thrillerGenre.GenreId });
                if (horrorGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = horrorGenre.GenreId });
            }
            else if (movie.Title.Contains("Die Hard") || movie.Title.Contains("Gladiator"))
            {
                if (actionGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = actionGenre.GenreId });
            }
            else if (movie.Title.Contains("Forrest") || movie.Title.Contains("Green Mile") || movie.Title.Contains("Schindler"))
            {
                if (dramaGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = dramaGenre.GenreId });
            }
            else
            {
                // Default: add action and drama
                if (actionGenre != null) movieGenres.Add(new MovieGenre { MovieId = movie.MovieId, GenreId = actionGenre.GenreId });
            }
        }

        if (movieGenres.Any())
        {
            await context.MovieGenres.AddRangeAsync(movieGenres);
            await context.SaveChangesAsync();
        }
    }
}