using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieRental.ViewModels.Movies;

public class MovieSearchViewModel
{
    // Filtros
    public string? SearchTerm { get; set; }
    public int? GenreId { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }

    // Resultados
    public List<MovieIndexViewModel> Movies { get; set; } = new();
    public int TotalCount { get; set; }

    // Para los dropdowns
    public SelectList? Genres { get; set; }
    public SelectList? SortOptions { get; set; }
    public SelectList? SortOrderOptions { get; set; }

    // Opciones de ordenamiento
    public static List<SelectListItem> GetSortOptions() => new()
    {
        new SelectListItem { Value = "title", Text = "Title" },
        new SelectListItem { Value = "year", Text = "Release Year" },
        new SelectListItem { Value = "price", Text = "Rental Price" },
        new SelectListItem { Value = "duration", Text = "Duration" },
        new SelectListItem { Value = "created", Text = "Date Added" }
    };

    public static List<SelectListItem> GetSortOrderOptions() => new()
    {
        new SelectListItem { Value = "asc", Text = "Ascending" },
        new SelectListItem { Value = "desc", Text = "Descending" }
    };

    // Helper para verificar si hay filtros activos
    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        GenreId.HasValue ||
        YearFrom.HasValue ||
        YearTo.HasValue ||
        PriceFrom.HasValue ||
        PriceTo.HasValue;
}