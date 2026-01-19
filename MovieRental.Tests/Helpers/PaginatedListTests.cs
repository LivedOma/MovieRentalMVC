using FluentAssertions;
using MovieRental.Helpers;

namespace MovieRental.Tests.Helpers;

public class PaginatedListTests
{
    [Fact]
    public void Create_ShouldReturnCorrectPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, pageIndex: 1, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
        result.PageIndex.Should().Be(1);
        result.TotalPages.Should().Be(10);
        result.TotalCount.Should().Be(100);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnCorrectMiddlePage()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, pageIndex: 5, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
        result.PageIndex.Should().Be(5);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
        result.First().Should().Be(41);
        result.Last().Should().Be(50);
    }

    [Fact]
    public void Create_ShouldReturnCorrectLastPage()
    {
        // Arrange
        var items = Enumerable.Range(1, 95).ToList();

        // Act
        var result = PaginatedList<int>.Create(items, pageIndex: 10, pageSize: 10);

        // Assert
        result.Should().HaveCount(5); // Solo 5 items en la última página
        result.PageIndex.Should().Be(10);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void FirstItemIndex_ShouldBeCorrect()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();

        // Act
        var page1 = PaginatedList<int>.Create(items, pageIndex: 1, pageSize: 10);
        var page3 = PaginatedList<int>.Create(items, pageIndex: 3, pageSize: 10);

        // Assert
        page1.FirstItemIndex.Should().Be(1);
        page3.FirstItemIndex.Should().Be(21);
    }

    [Fact]
    public void LastItemIndex_ShouldBeCorrect()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).ToList();

        // Act
        var page1 = PaginatedList<int>.Create(items, pageIndex: 1, pageSize: 10);
        var page3 = PaginatedList<int>.Create(items, pageIndex: 3, pageSize: 10);

        // Assert
        page1.LastItemIndex.Should().Be(10);
        page3.LastItemIndex.Should().Be(25); // Solo 5 items en la última página
    }

    [Fact]
    public void GetPageNumbers_ShouldReturnCorrectRange()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).ToList();
        var result = PaginatedList<int>.Create(items, pageIndex: 5, pageSize: 10);

        // Act
        var pageNumbers = result.GetPageNumbers(5).ToList();

        // Assert
        pageNumbers.Should().HaveCount(5);
        pageNumbers.Should().Contain(new[] { 3, 4, 5, 6, 7 });
    }

    [Fact]
    public void Create_WithEmptyList_ShouldHandleGracefully()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var result = PaginatedList<int>.Create(items, pageIndex: 1, pageSize: 10);

        // Assert
        result.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }
}