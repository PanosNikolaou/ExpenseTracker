using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Repositories;
using Xunit;

namespace ExpenseTracker.Tests;

/// <summary>
/// Unit tests for <see cref="ExpenseService"/>.
/// </summary>
public sealed class ExpenseServiceTests
{
    private static CreateExpenseRequest Request(
        string description = "Coffee beans",
        string category = "Groceries",
        decimal amount = 12.50m,
        DateOnly? date = null) =>
        new(description, category, amount, date ?? new DateOnly(2026, 11, 10));

    private static ExpenseService CreateService() => new(new InMemoryExpenseRepository());

    [Fact]
    public void AddExpense_PersistsExpense_AndReturnsView()
    {
        ExpenseService service = CreateService();

        ExpenseDto created = service.AddExpense(Request());

        Assert.Equal(1, created.Id);
        Assert.Equal("Coffee beans", created.Description);
        Assert.Equal(12.50m, created.Amount);
        Assert.Single(service.GetExpenses());
    }

    [Fact]
    public void AddExpense_AssignsSequentialIds()
    {
        ExpenseService service = CreateService();

        ExpenseDto first = service.AddExpense(Request());
        ExpenseDto second = service.AddExpense(Request());

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void AddExpense_WithNonPositiveAmount_ThrowsArgumentOutOfRangeException(decimal amount)
    {
        ExpenseService service = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.AddExpense(Request(amount: amount)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddExpense_WithEmptyDescription_ThrowsArgumentException(string description)
    {
        ExpenseService service = CreateService();

        Assert.Throws<ArgumentException>(
            () => service.AddExpense(Request(description: description)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddExpense_WithEmptyCategory_ThrowsArgumentException(string category)
    {
        ExpenseService service = CreateService();

        Assert.Throws<ArgumentException>(
            () => service.AddExpense(Request(category: category)));
    }

    [Fact]
    public void GetExpense_WithUnknownId_ReturnsNull()
    {
        ExpenseService service = CreateService();

        Assert.Null(service.GetExpense(99));
    }

    [Fact]
    public void DeleteExpense_WithExistingId_RemovesExpense()
    {
        ExpenseService service = CreateService();
        ExpenseDto created = service.AddExpense(Request());

        bool deleted = service.DeleteExpense(created.Id);

        Assert.True(deleted);
        Assert.Empty(service.GetExpenses());
    }

    [Fact]
    public void DeleteExpense_WithUnknownId_ReturnsFalse()
    {
        ExpenseService service = CreateService();

        Assert.False(service.DeleteExpense(99));
    }

    [Fact]
    public void GetMonthlySummary_SumsOnlyExpensesOfTheRequestedMonth()
    {
        ExpenseService service = CreateService();
        service.AddExpense(Request(category: "Groceries", date: new DateOnly(2026, 11, 2)));
        service.AddExpense(Request(category: "Fuel", date: new DateOnly(2026, 11, 15)));
        service.AddExpense(Request(category: "Rent", date: new DateOnly(2026, 10, 1)));

        MonthlySummaryDto summary = service.GetMonthlySummary(2026, 11);

        Assert.Equal(2, summary.ExpenseCount);
        Assert.Equal(25.00m, summary.TotalAmount);
    }

    [Fact]
    public void GetMonthlySummary_WithNoExpenses_ReturnsZeros()
    {
        ExpenseService service = CreateService();

        MonthlySummaryDto summary = service.GetMonthlySummary(2026, 1);

        Assert.Equal(0, summary.ExpenseCount);
        Assert.Equal(0m, summary.TotalAmount);
    }

    [Theory]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void GetMonthlySummary_WithInvalidMonth_ThrowsArgumentOutOfRangeException(int year, int month)
    {
        ExpenseService service = CreateService();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetMonthlySummary(year, month));
    }

    [Fact]
    public void UpdateExpense_WithExistingId_ReplacesExpense()
    {
        ExpenseService service = CreateService();
        ExpenseDto created = service.AddExpense(Request());
        UpdateExpenseRequest update = new("Tea leaves", "Drinks", 4.20m, new DateOnly(2026, 11, 12));

        ExpenseDto? updated = service.UpdateExpense(created.Id, update);

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Tea leaves", updated.Description);
        Assert.Equal("Drinks", updated.Category);
        Assert.Equal(4.20m, updated.Amount);
        Assert.Equal(new DateOnly(2026, 11, 12), updated.Date);

        ExpenseDto stored = Assert.Single(service.GetExpenses());
        Assert.Equal("Tea leaves", stored.Description);
    }

    [Fact]
    public void UpdateExpense_WithUnknownId_ReturnsNull_AndChangesNothing()
    {
        ExpenseService service = CreateService();

        Assert.Null(service.UpdateExpense(99, new UpdateExpenseRequest("Tea leaves", "Drinks", 4.20m, new DateOnly(2026, 11, 12))));
        Assert.Empty(service.GetExpenses());
    }

    [Fact]
    public void UpdateExpense_WithInvalidAmount_ThrowsArgumentOutOfRangeException()
    {
        ExpenseService service = CreateService();
        ExpenseDto created = service.AddExpense(Request());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.UpdateExpense(created.Id, new UpdateExpenseRequest("Tea leaves", "Drinks", 0m, new DateOnly(2026, 11, 12))));
    }

    [Fact]
    public void GetExpenses_WithCategoryFilter_ReturnsOnlyMatchingExpenses()
    {
        ExpenseService service = CreateService();
        service.AddExpense(Request(description: "Apples", category: "groceries"));
        service.AddExpense(Request(description: "Petrol", category: "Fuel"));

        IReadOnlyCollection<ExpenseDto> groceries = service.GetExpenses(category: "GROCERIES");

        ExpenseDto expense = Assert.Single(groceries);
        Assert.Equal("Apples", expense.Description);
    }

    [Fact]
    public void GetExpenses_WithDateRangeFilter_ReturnsOnlyExpensesInRange()
    {
        ExpenseService service = CreateService();
        service.AddExpense(Request(description: "Before", date: new DateOnly(2026, 11, 1)));
        service.AddExpense(Request(description: "Inside", date: new DateOnly(2026, 11, 10)));
        service.AddExpense(Request(description: "After", date: new DateOnly(2026, 11, 20)));

        IReadOnlyCollection<ExpenseDto> inRange =
            service.GetExpenses(from: new DateOnly(2026, 11, 5), to: new DateOnly(2026, 11, 15));

        ExpenseDto expense = Assert.Single(inRange);
        Assert.Equal("Inside", expense.Description);
    }

    [Fact]
    public void GetMonthlySummary_IncludesCategoryBreakdownOrderedByTotal()
    {
        ExpenseService service = CreateService();
        service.AddExpense(Request(category: "Rent", amount: 500m, date: new DateOnly(2026, 11, 1)));
        service.AddExpense(Request(category: "Groceries", amount: 30m, date: new DateOnly(2026, 11, 2)));
        service.AddExpense(Request(category: "Groceries", amount: 20m, date: new DateOnly(2026, 11, 3)));

        MonthlySummaryDto summary = service.GetMonthlySummary(2026, 11);

        Assert.Equal(2, summary.CategoryBreakdown.Count);
        Assert.Equal("Rent", summary.CategoryBreakdown[0].Category);
        Assert.Equal(500m, summary.CategoryBreakdown[0].TotalAmount);
        Assert.Equal(1, summary.CategoryBreakdown[0].ExpenseCount);
        Assert.Equal("Groceries", summary.CategoryBreakdown[1].Category);
        Assert.Equal(50m, summary.CategoryBreakdown[1].TotalAmount);
        Assert.Equal(2, summary.CategoryBreakdown[1].ExpenseCount);
    }
}
