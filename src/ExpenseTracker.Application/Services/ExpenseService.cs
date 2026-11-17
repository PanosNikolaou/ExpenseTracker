using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Domain;

namespace ExpenseTracker.Application.Services;

/// <summary>
/// Default implementation of the expense use cases, backed by a repository.
/// </summary>
public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;

    /// <summary>
    /// Initializes a new service instance.
    /// </summary>
    /// <param name="repository">Persistence abstraction used to store and retrieve expenses.</param>
    public ExpenseService(IExpenseRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public ExpenseDto AddExpense(CreateExpenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Expense expense = new(NextId(), request.Description, request.Category, request.Amount, request.Date);
        _repository.Add(expense);
        return MapToDto(expense);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ExpenseDto> GetExpenses(string? category = null, DateOnly? from = null, DateOnly? to = null) =>
        _repository
            .GetAll()
            .Where(expense => string.IsNullOrEmpty(category) || string.Equals(expense.Category, category, StringComparison.OrdinalIgnoreCase))
            .Where(expense => from is null || expense.Date >= from.Value)
            .Where(expense => to is null || expense.Date <= to.Value)
            .Select(MapToDto)
            .ToList();

    /// <inheritdoc />
    public ExpenseDto? GetExpense(int expenseId) =>
        _repository.FindById(expenseId) is { } expense ? MapToDto(expense) : null;

    /// <inheritdoc />
    public ExpenseDto? UpdateExpense(int expenseId, UpdateExpenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_repository.FindById(expenseId) is not { } existing)
        {
            return null;
        }

        // Constructing the replacement validates the new values before mutating the store.
        Expense replacement = new(existing.Id, request.Description, request.Category, request.Amount, request.Date);
        _repository.Remove(existing);
        _repository.Add(replacement);
        return MapToDto(replacement);
    }

    /// <inheritdoc />
    public bool DeleteExpense(int expenseId)
    {
        if (_repository.FindById(expenseId) is not { } expense)
        {
            return false;
        }

        _repository.Remove(expense);
        return true;
    }

    /// <inheritdoc />
    public MonthlySummaryDto GetMonthlySummary(int year, int month)
    {
        // Constructing a DateOnly validates the month range (1-12) for us.
        _ = new DateOnly(year, month, 1);

        IReadOnlyCollection<Expense> expenses = _repository.GetAll();
        List<Expense> inMonth = expenses.Where(expense => expense.Date.Year == year && expense.Date.Month == month).ToList();

        List<CategoryTotal> breakdown = inMonth
            .GroupBy(expense => expense.Category, StringComparer.OrdinalIgnoreCase)
            .Select(group => new CategoryTotal(group.Key, group.Sum(expense => expense.Amount), group.Count()))
            .OrderByDescending(categoryTotal => categoryTotal.TotalAmount)
            .ThenBy(categoryTotal => categoryTotal.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new MonthlySummaryDto(year, month, inMonth.Sum(expense => expense.Amount), inMonth.Count, breakdown);
    }

    private int NextId()
    {
        IReadOnlyCollection<Expense> expenses = _repository.GetAll();
        return expenses.Count == 0 ? 1 : expenses.Max(expense => expense.Id) + 1;
    }

    private static ExpenseDto MapToDto(Expense expense) =>
        new(expense.Id, expense.Description, expense.Category, expense.Amount, expense.Date);
}
