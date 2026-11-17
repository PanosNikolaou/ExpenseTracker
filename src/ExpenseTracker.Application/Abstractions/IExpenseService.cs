using ExpenseTracker.Application.Models;

namespace ExpenseTracker.Application.Abstractions;

/// <summary>
/// Coordinates the expense use cases: recording, querying, summarizing, and deleting expenses.
/// </summary>
public interface IExpenseService
{
    /// <summary>Records a new expense.</summary>
    /// <param name="request">The expense details supplied by the caller.</param>
    /// <returns>A view of the stored expense.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when description or category is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not positive.</exception>
    ExpenseDto AddExpense(CreateExpenseRequest request);

    /// <summary>Returns all recorded expenses, optionally filtered.</summary>
    /// <param name="category">When set, only expenses in this category (case-insensitive) are returned.</param>
    /// <param name="from">When set, only expenses on or after this date are returned (inclusive).</param>
    /// <param name="to">When set, only expenses on or before this date are returned (inclusive).</param>
    /// <returns>A read-only collection of the matching expenses.</returns>
    IReadOnlyCollection<ExpenseDto> GetExpenses(string? category = null, DateOnly? from = null, DateOnly? to = null);

    /// <summary>Finds an expense by its identifier.</summary>
    /// <param name="expenseId">Identifier of the expense to find.</param>
    /// <returns>A view of the expense, or <see langword="null"/> when none exists.</returns>
    ExpenseDto? GetExpense(int expenseId);

    /// <summary>Replaces an existing expense with new details.</summary>
    /// <param name="expenseId">Identifier of the expense to replace.</param>
    /// <param name="request">The new expense details supplied by the caller.</param>
    /// <returns>A view of the updated expense, or <see langword="null"/> when none exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when description or category is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not positive.</exception>
    ExpenseDto? UpdateExpense(int expenseId, UpdateExpenseRequest request);

    /// <summary>Deletes an expense.</summary>
    /// <param name="expenseId">Identifier of the expense to delete.</param>
    /// <returns><see langword="true"/> when the expense existed and was deleted; otherwise <see langword="false"/>.</returns>
    bool DeleteExpense(int expenseId);

    /// <summary>Computes aggregated totals for a calendar month.</summary>
    /// <param name="year">The calendar year to summarize.</param>
    /// <param name="month">The calendar month to summarize (1-12).</param>
    /// <returns>The monthly summary.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the month is outside 1-12.</exception>
    MonthlySummaryDto GetMonthlySummary(int year, int month);
}
