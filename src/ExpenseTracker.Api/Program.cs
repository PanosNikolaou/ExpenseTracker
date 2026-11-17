using ExpenseTracker.Api.Endpoints;
using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IExpenseRepository, InMemoryExpenseRepository>();
builder.Services.AddSingleton<IExpenseService, ExpenseService>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapExpenseEndpoints();

app.Run();

/// <summary>Exposes the generated <see cref="Program"/> class for use by integration test hosts.</summary>
public partial class Program;
