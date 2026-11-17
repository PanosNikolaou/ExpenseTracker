namespace ExpenseTracker.Domain;

/// <summary>
/// Represents a single recorded expense with a description, category, amount, and date.
/// </summary>
public sealed class Expense
{
    /// <summary>
    /// Initializes a new expense.
    /// </summary>
    /// <param name="id">Unique identifier of the expense.</param>
    /// <param name="description">Short description of what the expense was for; must not be empty.</param>
    /// <param name="category">Category the expense belongs to (for example "Groceries"); must not be empty.</param>
    /// <param name="amount">Amount spent; must be positive.</param>
    /// <param name="date">Calendar date the expense occurred.</param>
    /// <exception cref="ArgumentException">Thrown when description or category is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is not positive.</exception>
    public Expense(int id, string description, string category, decimal amount, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("The expense description must not be empty.", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("The expense category must not be empty.", nameof(category));
        }

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "The expense amount must be positive.");
        }

        Id = id;
        Description = description;
        Category = category;
        Amount = amount;
        Date = date;
    }

    /// <summary>Gets the unique identifier of the expense.</summary>
    public int Id { get; }

    /// <summary>Gets the description of the expense.</summary>
    public string Description { get; }

    /// <summary>Gets the category the expense belongs to.</summary>
    public string Category { get; }

    /// <summary>Gets the amount spent.</summary>
    public decimal Amount { get; }

    /// <summary>Gets the calendar date the expense occurred.</summary>
    public DateOnly Date { get; }
}
