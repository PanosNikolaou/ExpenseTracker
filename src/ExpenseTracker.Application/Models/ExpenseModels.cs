namespace ExpenseTracker.Application.Models;

/// <summary>
/// Request payload for recording a new expense.
/// </summary>
/// <param name="Description">Short description of what the expense was for.</param>
/// <param name="Category">Category the expense belongs to.</param>
/// <param name="Amount">Amount spent; must be positive.</param>
/// <param name="Date">Calendar date the expense occurred.</param>
public sealed record CreateExpenseRequest(string Description, string Category, decimal Amount, DateOnly Date);

/// <summary>
/// Request payload for replacing an existing expense.
/// </summary>
/// <param name="Description">Short description of what the expense was for.</param>
/// <param name="Category">Category the expense belongs to.</param>
/// <param name="Amount">Amount spent; must be positive.</param>
/// <param name="Date">Calendar date the expense occurred.</param>
public sealed record UpdateExpenseRequest(string Description, string Category, decimal Amount, DateOnly Date);

/// <summary>
/// Read-only view of a stored expense.
/// </summary>
/// <param name="Id">Unique identifier of the expense.</param>
/// <param name="Description">The description of the expense.</param>
/// <param name="Category">The category the expense belongs to.</param>
/// <param name="Amount">The amount spent.</param>
/// <param name="Date">The calendar date the expense occurred.</param>
public sealed record ExpenseDto(int Id, string Description, string Category, decimal Amount, DateOnly Date);

/// <summary>
/// Aggregated totals for a single calendar month.
/// </summary>
/// <param name="Year">The calendar year of the summary.</param>
/// <param name="Month">The calendar month of the summary (1-12).</param>
/// <param name="TotalAmount">Sum of all expense amounts in the month.</param>
/// <param name="ExpenseCount">Number of expenses recorded in the month.</param>
/// <param name="CategoryBreakdown">Totals per category for the month, highest total first.</param>
public sealed record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalAmount,
    int ExpenseCount,
    IReadOnlyList<CategoryTotal> CategoryBreakdown);

/// <summary>
/// Aggregated total for a single category.
/// </summary>
/// <param name="Category">The category name.</param>
/// <param name="TotalAmount">Sum of all expense amounts in the category.</param>
/// <param name="ExpenseCount">Number of expenses recorded in the category.</param>
public sealed record CategoryTotal(string Category, decimal TotalAmount, int ExpenseCount);
