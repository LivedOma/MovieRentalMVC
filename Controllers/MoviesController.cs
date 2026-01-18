using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;
using MovieRental.ViewModels.Movies;

namespace MovieRental.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Movies
    public async Task<IActionResult> Index()
    {
        var movies = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MovieIndexViewModel
            {
                MovieId = m.MovieId,
                Title = m.Title,
                ReleaseYear = m.ReleaseYear,
                DurationMinutes = m.DurationMinutes,
                RentalPrice = m.RentalPrice,
                Genres = m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
            })
            .ToListAsync();

        return View(movies);
    }

    // GET: Movies/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieCasts)
                .ThenInclude(mc => mc.Person)
            .Include(m => m.MovieCrews)
                .ThenInclude(mc => mc.Person)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie == null) return NotFound();

        var viewModel = new MovieDetailsViewModel
        {
            MovieId = movie.MovieId,
            Title = movie.Title,
            OriginalTitle = movie.OriginalTitle,
            Synopsis = movie.Synopsis,
            ReleaseYear = movie.ReleaseYear,
            DurationMinutes = movie.DurationMinutes,
            Language = movie.Language,
            RentalPrice = movie.RentalPrice,
            CreatedAt = movie.CreatedAt,
            Genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList(),
            Cast = movie.MovieCasts
                .OrderBy(mc => mc.CastOrder)
                .Select(mc => new CastMemberViewModel
                {
                    ActorName = mc.Person.FullName,
                    CharacterName = mc.CharacterName,
                    CastOrder = mc.CastOrder
                }).ToList(),
            Crew = movie.MovieCrews
                .Select(mc => new CrewMemberViewModel
                {
                    PersonName = mc.Person.FullName,
                    Role = mc.Role
                }).ToList()
        };

        return View(viewModel);
    }

    // GET: Movies/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new MovieFormViewModel
        {
            ReleaseYear = DateTime.Now.Year,
            AvailableGenres = await GetGenreCheckboxList()
        };

        return View(viewModel);
    }

    // POST: Movies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieFormViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var movie = new Movie
            {
                Title = viewModel.Title,
                OriginalTitle = viewModel.OriginalTitle,
                Synopsis = viewModel.Synopsis,
                ReleaseYear = viewModel.ReleaseYear,
                DurationMinutes = viewModel.DurationMinutes,
                Language = viewModel.Language,
                RentalPrice = viewModel.RentalPrice,
                CreatedAt = DateTime.UtcNow
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Agregar géneros seleccionados
            if (viewModel.SelectedGenreIds.Any())
            {
                var movieGenres = viewModel.SelectedGenreIds
                    .Select(genreId => new MovieGenre
                    {
                        MovieId = movie.MovieId,
                        GenreId = genreId
                    });

                _context.MovieGenres.AddRange(movieGenres);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie created successfully!";
            return RedirectToAction(nameof(Index));
        }

        viewModel.AvailableGenres = await GetGenreCheckboxList(viewModel.SelectedGenreIds);
        return View(viewModel);
    }

    // GET: Movies/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie == null) return NotFound();

        var selectedGenreIds = movie.MovieGenres.Select(mg => mg.GenreId).ToList();

        var viewModel = new MovieFormViewModel
        {
            MovieId = movie.MovieId,
            Title = movie.Title,
            OriginalTitle = movie.OriginalTitle,
            Synopsis = movie.Synopsis,
            ReleaseYear = movie.ReleaseYear,
            DurationMinutes = movie.DurationMinutes,
            Language = movie.Language,
            RentalPrice = movie.RentalPrice,
            SelectedGenreIds = selectedGenreIds,
            AvailableGenres = await GetGenreCheckboxList(selectedGenreIds)
        };

        return View(viewModel);
    }

    // POST: Movies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MovieFormViewModel viewModel)
    {
        if (id != viewModel.MovieId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var movie = await _context.Movies
                    .Include(m => m.MovieGenres)
                    .FirstOrDefaultAsync(m => m.MovieId == id);

                if (movie == null) return NotFound();

                // Actualizar propiedades
                movie.Title = viewModel.Title;
                movie.OriginalTitle = viewModel.OriginalTitle;
                movie.Synopsis = viewModel.Synopsis;
                movie.ReleaseYear = viewModel.ReleaseYear;
                movie.DurationMinutes = viewModel.DurationMinutes;
                movie.Language = viewModel.Language;
                movie.RentalPrice = viewModel.RentalPrice;

                // Actualizar géneros: eliminar existentes y agregar nuevos
                _context.MovieGenres.RemoveRange(movie.MovieGenres);

                if (viewModel.SelectedGenreIds.Any())
                {
                    var newMovieGenres = viewModel.SelectedGenreIds
                        .Select(genreId => new MovieGenre
                        {
                            MovieId = movie.MovieId,
                            GenreId = genreId
                        });

                    _context.MovieGenres.AddRange(newMovieGenres);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Movie updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(viewModel.MovieId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        viewModel.AvailableGenres = await GetGenreCheckboxList(viewModel.SelectedGenreIds);
        return View(viewModel);
    }

    // GET: Movies/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.MovieId == id);

        if (movie == null) return NotFound();

        var viewModel = new MovieDetailsViewModel
        {
            MovieId = movie.MovieId,
            Title = movie.Title,
            ReleaseYear = movie.ReleaseYear,
            DurationMinutes = movie.DurationMinutes,
            RentalPrice = movie.RentalPrice,
            Genres = movie.MovieGenres.Select(mg => mg.Genre.Name).ToList()
        };

        return View(viewModel);
    }

    // POST: Movies/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie != null)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Movie deleted successfully!";
        }

        return RedirectToAction(nameof(Index));
    }

    // Helper Methods
    private bool MovieExists(int id)
    {
        return _context.Movies.Any(e => e.MovieId == id);
    }

    private async Task<List<GenreCheckboxViewModel>> GetGenreCheckboxList(List<int>? selectedIds = null)
    {
        selectedIds ??= new List<int>();

        return await _context.Genres
            .OrderBy(g => g.Name)
            .Select(g => new GenreCheckboxViewModel
            {
                GenreId = g.GenreId,
                Name = g.Name,
                IsSelected = selectedIds.Contains(g.GenreId)
            })
            .ToListAsync();
    }
}