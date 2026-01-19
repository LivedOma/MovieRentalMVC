using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieRental.Data;
using MovieRental.Models.Movies;
using MovieRental.ViewModels.Movies;
using Microsoft.AspNetCore.Mvc.Rendering;
using MovieRental.Helpers;

namespace MovieRental.Controllers;

public class MoviesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MoviesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Movies
    public async Task<IActionResult> Index(
        string? searchTerm,
        int? genreId,
        int? yearFrom,
        int? yearTo,
        decimal? priceFrom,
        decimal? priceTo,
        string? sortBy,
        string? sortOrder,
        int page = 1,
        int pageSize = 12)
    {
        // Validar parámetros de paginación
        page = Math.Max(1, page);
        pageSize = new[] { 6, 12, 24, 48 }.Contains(pageSize) ? pageSize : 12;

        // Query base
        var query = _context.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .AsQueryable();

        // Filtro por término de búsqueda (título, título original, sinopsis)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(m =>
                m.Title.ToLower().Contains(term) ||
                (m.OriginalTitle != null && m.OriginalTitle.ToLower().Contains(term)) ||
                (m.Synopsis != null && m.Synopsis.ToLower().Contains(term)));
        }

        // Filtro por género
        if (genreId.HasValue)
        {
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        // Filtro por rango de años
        if (yearFrom.HasValue)
        {
            query = query.Where(m => m.ReleaseYear >= yearFrom.Value);
        }
        if (yearTo.HasValue)
        {
            query = query.Where(m => m.ReleaseYear <= yearTo.Value);
        }

        // Filtro por rango de precios
        if (priceFrom.HasValue)
        {
            query = query.Where(m => m.RentalPrice >= priceFrom.Value);
        }
        if (priceTo.HasValue)
        {
            query = query.Where(m => m.RentalPrice <= priceTo.Value);
        }

        // Ordenamiento
        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("title", "desc") => query.OrderByDescending(m => m.Title),
            ("title", _) => query.OrderBy(m => m.Title),
            ("year", "asc") => query.OrderBy(m => m.ReleaseYear),
            ("year", _) => query.OrderByDescending(m => m.ReleaseYear),
            ("price", "desc") => query.OrderByDescending(m => m.RentalPrice),
            ("price", _) => query.OrderBy(m => m.RentalPrice),
            ("duration", "desc") => query.OrderByDescending(m => m.DurationMinutes),
            ("duration", _) => query.OrderBy(m => m.DurationMinutes),
            ("created", "asc") => query.OrderBy(m => m.CreatedAt),
            _ => query.OrderByDescending(m => m.CreatedAt) // Default: más recientes primero
        };

        // Proyectar a ViewModel
        var projectedQuery = query.Select(m => new MovieIndexViewModel
        {
            MovieId = m.MovieId,
            Title = m.Title,
            ReleaseYear = m.ReleaseYear,
            DurationMinutes = m.DurationMinutes,
            RentalPrice = m.RentalPrice,
            Genres = m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
        });

        // Aplicar paginación
        var paginatedMovies = await PaginatedList<MovieIndexViewModel>
            .CreateAsync(projectedQuery, page, pageSize);

        // Preparar géneros para el dropdown
        var genres = await _context.Genres
            .OrderBy(g => g.Name)
            .ToListAsync();

        // Crear ViewModel
        var viewModel = new MovieSearchViewModel
        {
            SearchTerm = searchTerm,
            GenreId = genreId,
            YearFrom = yearFrom,
            YearTo = yearTo,
            PriceFrom = priceFrom,
            PriceTo = priceTo,
            SortBy = sortBy ?? "created",
            SortOrder = sortOrder ?? "desc",
            PageIndex = page,
            PageSize = pageSize,
            Movies = paginatedMovies,
            Genres = new SelectList(genres, "GenreId", "Name", genreId),
            SortOptions = new SelectList(MovieSearchViewModel.GetSortOptions(), "Value", "Text", sortBy ?? "created"),
            SortOrderOptions = new SelectList(MovieSearchViewModel.GetSortOrderOptions(), "Value", "Text", sortOrder ?? "desc"),
            PageSizeOptions = new SelectList(MovieSearchViewModel.GetPageSizeOptions(), "Value", "Text", pageSize.ToString())
        };

        return View(viewModel);
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
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