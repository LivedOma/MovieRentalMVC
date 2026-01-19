using FluentAssertions;
using MovieRental.ViewModels.Movies;

namespace MovieRental.Tests.ViewModels;

public class MovieSearchViewModelTests
{
    [Fact]
    public void HasActiveFilters_WithNoFilters_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel();

        // Act & Assert
        viewModel.HasActiveFilters.Should().BeFalse();
    }

    [Fact]
    public void HasActiveFilters_WithSearchTerm_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel { SearchTerm = "Inception" };

        // Act & Assert
        viewModel.HasActiveFilters.Should().BeTrue();
    }

    [Fact]
    public void HasActiveFilters_WithGenreId_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel { GenreId = 1 };

        // Act & Assert
        viewModel.HasActiveFilters.Should().BeTrue();
    }

    [Fact]
    public void HasActiveFilters_WithYearRange_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel { YearFrom = 2000, YearTo = 2020 };

        // Act & Assert
        viewModel.HasActiveFilters.Should().BeTrue();
    }

    [Fact]
    public void HasActiveFilters_WithPriceRange_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel { PriceFrom = 1.99m, PriceTo = 9.99m };

        // Act & Assert
        viewModel.HasActiveFilters.Should().BeTrue();
    }

    [Fact]
    public void GetQueryString_WithNoFilters_ShouldReturnEmpty()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            PageIndex = 1,
            PageSize = 12
        };

        // Act
        var result = viewModel.GetQueryString();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetQueryString_WithSearchTerm_ShouldIncludeSearchTerm()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            SearchTerm = "Inception",
            PageIndex = 1,
            PageSize = 12
        };

        // Act
        var result = viewModel.GetQueryString();

        // Assert
        result.Should().Contain("searchTerm=Inception");
    }

    [Fact]
    public void GetQueryString_WithPage_ShouldIncludePage()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            PageIndex = 3,
            PageSize = 12
        };

        // Act
        var result = viewModel.GetQueryString();

        // Assert
        result.Should().Contain("page=3");
    }

    [Fact]
    public void GetQueryString_WithNonDefaultPageSize_ShouldIncludePageSize()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            PageIndex = 1,
            PageSize = 24
        };

        // Act
        var result = viewModel.GetQueryString();

        // Assert
        result.Should().Contain("pageSize=24");
    }

    [Fact]
    public void GetQueryString_WithMultipleFilters_ShouldIncludeAll()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            SearchTerm = "Action",
            GenreId = 1,
            YearFrom = 2000,
            YearTo = 2020,
            SortBy = "title",
            SortOrder = "asc",
            PageIndex = 2,
            PageSize = 12
        };

        // Act
        var result = viewModel.GetQueryString();

        // Assert
        result.Should().Contain("searchTerm=Action");
        result.Should().Contain("genreId=1");
        result.Should().Contain("yearFrom=2000");
        result.Should().Contain("yearTo=2020");
        result.Should().Contain("sortBy=title");
        result.Should().Contain("sortOrder=asc");
        result.Should().Contain("page=2");
    }

    [Fact]
    public void GetQueryString_WithCustomPage_ShouldUseCustomPage()
    {
        // Arrange
        var viewModel = new MovieSearchViewModel
        {
            SearchTerm = "Test",
            PageIndex = 1,
            PageSize = 12
        };

        // Act
        var result = viewModel.GetQueryString(page: 5);

        // Assert
        result.Should().Contain("page=5");
    }

    [Fact]
    public void GetSortOptions_ShouldReturnAllOptions()
    {
        // Act
        var options = MovieSearchViewModel.GetSortOptions();

        // Assert
        options.Should().HaveCount(5);
        options.Should().Contain(o => o.Value == "title");
        options.Should().Contain(o => o.Value == "year");
        options.Should().Contain(o => o.Value == "price");
        options.Should().Contain(o => o.Value == "duration");
        options.Should().Contain(o => o.Value == "created");
    }

    [Fact]
    public void GetPageSizeOptions_ShouldReturnAllOptions()
    {
        // Act
        var options = MovieSearchViewModel.GetPageSizeOptions();

        // Assert
        options.Should().HaveCount(4);
        options.Should().Contain(o => o.Value == "6");
        options.Should().Contain(o => o.Value == "12");
        options.Should().Contain(o => o.Value == "24");
        options.Should().Contain(o => o.Value == "48");
    }
}