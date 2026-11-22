# Combinations API

ASP.NET Core (.NET 8) API that walks the permutations of 1..n in lexicographic order, per user session.

## Run

```
dotnet run --project Combinations
```

Swagger is available in Development at `/swagger`.

## Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/combinations/start?n=` | Start a session; returns the total (n!) |
| GET | `/api/combinations/next` | Next permutation |
| GET | `/api/combinations/all?pageSize=&page=` | Paged list, starting from the next unseen permutation |
| GET | `/api/combinations/back` | Last permutation shown |
| POST | `/api/combinations/reset` | Clear the session state |

## Structure

- `Combinations` – Web API (controller, middleware, DI).
- `Combinations.Core` – state model and the stateless `CombinationService` / permutation algorithms.
- `Combinations.Infrastructure` – session management (cookie-based session id, `ISessionStore`, in-memory store, cleanup service).

## Design notes

See [docs/Answers.docx](docs/Answers.docx).


