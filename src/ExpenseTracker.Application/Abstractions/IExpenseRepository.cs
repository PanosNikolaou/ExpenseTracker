using ExpenseTracker.Domain;

namespace ExpenseTracker.Application.Abstractions;

/// <summary>
/// Provides persistence for <see cref="Expense"/> instances.
/// </summary>
public interface IExpenseRepository
{
    /// <summary>Appends a new expense to the store.</summary>
    /// <param name="expense">The expense to store.</param>
    void Add(Expense expense);

    /// <summary>Returns every expense currently stored.</summary>
    /// <returns>A read-only snapshot of all stored expenses.</returns>
    IReadOnlyCollection<Expense> GetAll();

    /// <summary>Finds an expense by its identifier.</summary>
    /// <param name="id">The identifier to look up.</param>
    /// <returns>The matching expense, or <see langword="null"/> when no expense exists for the identifier.</returns>
    Expense? FindById(int id);

    /// <summary>Removes an expense from the store.</summary>
    /// <param name="expense">The expense to remove.</param>
    void Remove(Expense expense);
}
