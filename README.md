# ExpenseTracker

A simple expense tracking API built with **.NET 10** and **ASP.NET Core Minimal APIs**, following a clean architecture layout.

## Project structure

| Project | Description |
| --- | --- |
| `src/ExpenseTracker.Domain` | Domain entity (`Expense`) with built-in validation |
| `src/ExpenseTracker.Application` | Use cases (`ExpenseService`), abstractions, and DTOs |
| `src/ExpenseTracker.Infrastructure` | Persistence (in-memory repository, thread-safe) |
| `src/ExpenseTracker.Api` | Minimal API endpoints with OpenAPI support |
| `tests/ExpenseTracker.Tests` | Unit tests for the domain and service layers |
| `tests/ExpenseTracker.Api.Tests` | Integration tests over the HTTP API |

## API endpoints

All routes are grouped under `/expenses`. In development the OpenAPI document is available at `/openapi/v1.json`.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/expenses` | Lists all expenses. Optional filters: `category` (case-insensitive), `from`, `to` (inclusive, `yyyy-MM-dd`) |
| `GET` | `/expenses/{id}` | Finds a single expense, `404` when missing |
| `GET` | `/expenses/summary/{year}/{month}` | Monthly totals plus a per-category breakdown (highest total first) |
| `POST` | `/expenses` | Records a new expense; returns `201 Created` with a `Location` header |
| `PUT` | `/expenses/{id}` | Replaces an existing expense; returns `404` when missing |
| `DELETE` | `/expenses/{id}` | Deletes an expense; `204` on success, `404` when missing |

Invalid input (empty description/category, non-positive amount, invalid month) produces a `400` validation problem response.

## Running

```bash
dotnet run --project src/ExpenseTracker.Api
```

## Testing

```bash
dotnet test ExpenseTracker.slnx
```

## Notes

- The in-memory repository stores data for the lifetime of the process; swapping in a persistent implementation only requires a new `IExpenseRepository` registration.
- Expense identifiers are assigned sequentially by the service.
