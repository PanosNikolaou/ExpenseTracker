using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Domain;

namespace ExpenseTracker.Infrastructure.Repositories;

/// <summary>
/// Stores expenses in memory for the lifetime of the process.
/// </summary>
/// <remarks>
/// Intended as a simple default store; replace with a persistent implementation
/// without changing consumers of <see cref="IExpenseRepository"/>.
/// </remarks>
public sealed class InMemoryExpenseRepository : IExpenseRepository
{
    private readonly object _lock = new();
    private readonly List<Expense> _expenses = [];

    /// <inheritdoc />
    public void Add(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);

        lock (_lock)
        {
            _expenses.Add(expense);
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Expense> GetAll()
    {
        lock (_lock)
        {
            return _expenses.ToArray();
        }
    }

    /// <inheritdoc />
    public Expense? FindById(int id)
    {
        lock (_lock)
        {
            return _expenses.FirstOrDefault(expense => expense.Id == id);
        }
    }

    /// <inheritdoc />
    public void Remove(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);

        lock (_lock)
        {
            _expenses.Remove(expense);
        }
    }
}
