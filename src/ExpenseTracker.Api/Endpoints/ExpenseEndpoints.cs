using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExpenseTracker.Api.Endpoints;

/// <summary>
/// Maps the HTTP endpoints for managing expenses.
/// </summary>
public static class ExpenseEndpoints
{
    /// <summary>
    /// Registers all expense routes on the given endpoint route builder.
    /// </summary>
    /// <param name="app">The route builder to register the routes on.</param>
    /// <returns>The expense route group, enabling further route customization by callers.</returns>
    public static RouteGroupBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/expenses").WithTags("Expenses");

        group.MapGet("/", ListExpenses)
            .WithName("ListExpenses")
            .WithSummary("Lists all recorded expenses, optionally filtered by category and date range.");

        group.MapGet("/{expenseId:int}", GetExpense)
            .WithName("GetExpense")
            .WithSummary("Finds a single expense by id.");

        group.MapGet("/summary/{year:int}/{month:int}", GetMonthlySummary)
            .WithName("GetMonthlySummary")
            .WithSummary("Computes aggregated totals for a calendar month.");

        group.MapPost("/", CreateExpense)
            .WithName("CreateExpense")
            .WithSummary("Records a new expense.");

        group.MapPut("/{expenseId:int}", UpdateExpense)
            .WithName("UpdateExpense")
            .WithSummary("Replaces an existing expense.");

        group.MapDelete("/{expenseId:int}", DeleteExpense)
            .WithName("DeleteExpense")
            .WithSummary("Deletes an expense by id.");

        return group;
    }

    private static Ok<IReadOnlyCollection<ExpenseDto>> ListExpenses(
        IExpenseService expenseService,
        string? category = null,
        DateOnly? from = null,
        DateOnly? to = null) =>
        TypedResults.Ok(expenseService.GetExpenses(category, from, to));

    private static Results<Ok<ExpenseDto>, NotFound> GetExpense(int expenseId, IExpenseService expenseService) =>
        expenseService.GetExpense(expenseId) is { } expense
            ? TypedResults.Ok(expense)
            : TypedResults.NotFound();

    private static Results<Created<ExpenseDto>, ValidationProblem> CreateExpense(
        CreateExpenseRequest request,
        IExpenseService expenseService)
    {
        try
        {
            ExpenseDto created = expenseService.AddExpense(request);
            return TypedResults.Created($"/expenses/{created.Id}", created);
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException)
        {
            return TypedResults.ValidationProblem(ToValidationErrors(exception));
        }
    }

    private static Results<Ok<ExpenseDto>, NotFound, ValidationProblem> UpdateExpense(
        int expenseId,
        UpdateExpenseRequest request,
        IExpenseService expenseService)
    {
        try
        {
            return expenseService.UpdateExpense(expenseId, request) is { } updated
                ? TypedResults.Ok(updated)
                : TypedResults.NotFound();
        }
        catch (Exception exception) when (exception is ArgumentException or ArgumentOutOfRangeException)
        {
            return TypedResults.ValidationProblem(ToValidationErrors(exception));
        }
    }

    private static Results<NoContent, NotFound> DeleteExpense(int expenseId, IExpenseService expenseService) =>
        expenseService.DeleteExpense(expenseId)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();

    private static Results<Ok<MonthlySummaryDto>, ValidationProblem> GetMonthlySummary(
        int year,
        int month,
        IExpenseService expenseService)
    {
        try
        {
            return TypedResults.Ok(expenseService.GetMonthlySummary(year, month));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return TypedResults.ValidationProblem(ToValidationErrors(exception));
        }
    }

    private static Dictionary<string, string[]> ToValidationErrors(Exception exception)
    {
        string fieldName = exception is ArgumentException argumentException && argumentException.ParamName is not null
            ? argumentException.ParamName
            : "request";
        return new Dictionary<string, string[]> { [fieldName] = [exception.Message] };
    }
}
