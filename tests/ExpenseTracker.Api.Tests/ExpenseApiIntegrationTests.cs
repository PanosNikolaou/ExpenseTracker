using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseTracker.Api.Tests;

/// <summary>
/// Integration tests exercising the HTTP API through <see cref="WebApplicationFactory{TProgram}"/>.
/// Each test boots its own host so the in-memory store starts empty.
/// </summary>
public sealed class ExpenseApiIntegrationTests
{
    private static readonly DateOnly Date = new(2026, 11, 10);

    [Fact]
    public async Task CreateExpense_WithValidBody_ReturnsCreated_WithLocationHeader()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/expenses", Request());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/expenses/1", response.Headers.Location?.ToString());

        ExpenseDto? created = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        Assert.NotNull(created);
        Assert.Equal(1, created.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task CreateExpense_WithNonPositiveAmount_ReturnsValidationProblem(decimal amount)
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/expenses", Request(amount: amount));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("amount", problem.Errors.Keys);
    }

    [Fact]
    public async Task CreateExpense_WithEmptyDescription_ReturnsValidationProblem()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/expenses", Request(description: "   "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails? problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("description", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetExpense_WithUnknownId_ReturnsNotFound()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.GetAsync("/expenses/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExpense_WithExistingId_ReturnsUpdatedExpense()
    {
        using HttpClient client = CreateClient();
        ExpenseDto created = await CreateExpenseAsync(client);
        UpdateExpenseRequest update = new("Tea leaves", "Drinks", 4.20m, new DateOnly(2026, 11, 12));

        HttpResponseMessage response = await client.PutAsJsonAsync($"/expenses/{created.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ExpenseDto? updated = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Tea leaves", updated.Description);
    }

    [Fact]
    public async Task UpdateExpense_WithUnknownId_ReturnsNotFound()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync("/expenses/99", Request());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListExpenses_WithCategoryAndDateFilters_ReturnsOnlyMatchingExpenses()
    {
        using HttpClient client = CreateClient();
        await client.PostAsJsonAsync("/expenses", Request(description: "Apples", category: "Groceries", date: new DateOnly(2026, 11, 2)));
        await client.PostAsJsonAsync("/expenses", Request(description: "Petrol", category: "Fuel", date: new DateOnly(2026, 11, 3)));
        await client.PostAsJsonAsync("/expenses", Request(description: "Old rent", category: "Groceries", date: new DateOnly(2026, 10, 1)));

        HttpResponseMessage response = await client.GetAsync("/expenses?category=groceries&from=2026-11-01&to=2026-11-30");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ExpenseDto>? expenses = await response.Content.ReadFromJsonAsync<List<ExpenseDto>>();
        Assert.NotNull(expenses);
        ExpenseDto expense = Assert.Single(expenses);
        Assert.Equal("Apples", expense.Description);
    }

    [Fact]
    public async Task GetMonthlySummary_ReturnsTotalsWithCategoryBreakdown()
    {
        using HttpClient client = CreateClient();
        await client.PostAsJsonAsync("/expenses", Request(category: "Rent", amount: 500m, date: new DateOnly(2026, 11, 1)));
        await client.PostAsJsonAsync("/expenses", Request(category: "Groceries", amount: 30m, date: new DateOnly(2026, 11, 2)));

        HttpResponseMessage response = await client.GetAsync("/expenses/summary/2026/11");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        MonthlySummaryDto? summary = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        Assert.NotNull(summary);
        Assert.Equal(530m, summary.TotalAmount);
        Assert.Equal(2, summary.ExpenseCount);
        Assert.Equal("Rent", summary.CategoryBreakdown[0].Category);
        Assert.Equal(500m, summary.CategoryBreakdown[0].TotalAmount);
    }

    [Fact]
    public async Task GetMonthlySummary_WithInvalidMonth_ReturnsValidationProblem()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.GetAsync("/expenses/summary/2026/13");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_WithExistingId_ReturnsNoContent_AndExpenseIsGone()
    {
        using HttpClient client = CreateClient();
        ExpenseDto created = await CreateExpenseAsync(client);

        HttpResponseMessage delete = await client.DeleteAsync($"/expenses/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/expenses/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task DeleteExpense_WithUnknownId_ReturnsNotFound()
    {
        using HttpClient client = CreateClient();

        HttpResponseMessage response = await client.DeleteAsync("/expenses/99");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static HttpClient CreateClient()
    {
        WebApplicationFactory<Program> factory = new();
        return factory.CreateClient();
    }

    private static CreateExpenseRequest Request(
        string description = "Coffee beans",
        string category = "Groceries",
        decimal amount = 12.50m,
        DateOnly? date = null) =>
        new(description, category, amount, date ?? Date);

    private static async Task<ExpenseDto> CreateExpenseAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/expenses", Request());
        response.EnsureSuccessStatusCode();
        ExpenseDto? created = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        Assert.NotNull(created);
        return created;
    }
}
