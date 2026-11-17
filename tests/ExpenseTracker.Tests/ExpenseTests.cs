using ExpenseTracker.Domain;
using Xunit;

namespace ExpenseTracker.Tests;

/// <summary>
/// Unit tests for the <see cref="Expense"/> domain entity.
/// </summary>
public sealed class ExpenseTests
{
    private static readonly DateOnly Date = new(2026, 11, 10);

    [Fact]
    public void Constructor_SetsAllValues()
    {
        Expense expense = new(1, "Coffee beans", "Groceries", 12.50m, Date);

        Assert.Equal(1, expense.Id);
        Assert.Equal("Coffee beans", expense.Description);
        Assert.Equal("Groceries", expense.Category);
        Assert.Equal(12.50m, expense.Amount);
        Assert.Equal(Date, expense.Date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyDescription(string description)
    {
        Assert.Throws<ArgumentException>(
            () => new Expense(1, description, "Groceries", 12.50m, Date));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptyCategory(string category)
    {
        Assert.Throws<ArgumentException>(
            () => new Expense(1, "Coffee beans", category, 12.50m, Date));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonPositiveAmount(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Expense(1, "Coffee beans", "Groceries", amount, Date));
    }
}
