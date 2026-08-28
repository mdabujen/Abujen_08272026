\# File Processing Service



A secure ASP.NET Core Web API that accepts CSV file uploads, computes an average of a numeric column, and tracks every processing attempt for reporting. Built on .NET 10, secured with API key authentication, and containerized with Docker.



\## Architecture



The request flow follows three layers, each with a single responsibility:



Request -> \[API Key Middleware] -> \[Controllers] -> \[Services]



\- \*\*Middleware\*\* (`ApiKeyMiddleware`) validates the `X-Api-Key` header before any request reaches a controller.

\- \*\*Controllers\*\* (`FilesController`, `ReportsController`) stay thin — they validate input, call a service, and shape the HTTP response. No business logic lives here.

\- \*\*Services\*\* (`CsvProcessingService`, `FileTrackingService`) do the actual work: parsing CSV files and computing averages, and recording/reporting on every processing attempt.



Both services are exposed through interfaces (`ICsvProcessingService`, `IFileTrackingService`), so the underlying implementation (e.g. swapping in-memory tracking for a database), can change without touching the controllers that depend on them.



\## Prerequisites



\- .NET 10 SDK

\- Visual Studio 2026

\- Docker Desktop



\## Running Locally (Visual Studio)



1\. Clone the repository and open the solution file.

2\. Set your API key in `FileProcessingService/appsettings.Development.json`:

json

&#x20;  {

&#x20;    "ApiKey": "your-own-key-here"

&#x20;  }



&#x20;  `appsettings.json` (the committed base config) contains a placeholder value only — the real key is kept out of source control on purpose.

3. Click \*\*Authorize\*\* (top right), enter your API key, and click \*\*Authorize\*\* again to apply it to all requests.



\## Running via Docker



Build the image from the project folder (where `Dockerfile` lives):

powershell

docker build -t file-processing-service .



Run the container:

powershell

docker run -p 8080:8080 -e ApiKey=your-own-key-here -e ASPNETCORE\_ENVIRONMENT=Development file-processing-service



> \*\*Note:\*\* `ASPNETCORE\_ENVIRONMENT=Development` is required here because Swagger only registers itself in Development, containers default to Production, where Swagger is intentionally not exposed. This is deliberate: you don't want interactive API docs enabled on a real production deployment.



Then open:

http://localhost:8080/swagger



\## API Reference



All endpoints require an `X-Api-Key` header.



| Method | Route                  | Description                                                                                                                                                  |

| POST   | `/api/files/upload`    | Upload a CSV file. Optional `?column=Name` query param to specify which column to average; if omitted, the first fully numeric column is used automatically. |

| GET    | `/api/reports/summary` | Returns aggregate stats: total files processed, success/failure counts, total rows processed, last processed timestamp.                                      |

| GET    | `/api/reports/history` | Returns recent processing records, most recent first. Optional `?take=n` (default 50, max 500).                                                              |



\*\*Successful upload response (200):\*\*

json

{

&#x20; "recordId": "a4f86804-0067-4dfd-8915-79251f6c6249",

&#x20; "fileName": "sample.csv",

&#x20; "rowsProcessed": 5,

&#x20; "columnAnalyzed": "Amount",

&#x20; "average": 96.298

}



\*\*Failed upload response (400):\*\*

json

{

&#x20; "message": "Failed to process the uploaded file.",

&#x20; "detail": "Requested column 'Price' was not found in the CSV header."

}



\*\*Missing/invalid API key (401):\*\* returned by the middleware before the request reaches any controller.



\## File Tracking \& Reporting



Every processing attempt — success or failure — is recorded, not just successes. This is intentional: a reporting feature that only shows what worked gives an incomplete picture of what the service actually did.



Each tracked record includes:

\- File name and size

\- Rows processed and which column was analyzed

\- The calculated average

\- Processing duration (milliseconds)

\- Timestamp (UTC)

\- Success flag, and an error message if it failed



Tracking data is stored \*\*in memory\*\*, using a thread-safe `ConcurrentQueue`, so it's safe under concurrent uploads. It resets whenever the application restarts, there's no persistent store behind it. This was a deliberate scope decision for this exercise (see below); the tracking service sits behind an interface specifically so it could be swapped for a database-backed implementation without any change to the controllers that use it.



\## Design Decisions \& Trade-offs



\- \*\*CSV over JSON\*\*: a numeric-column average is a clean, easily verifiable aggregate to demonstrate.

\- \*\*In-memory tracking, not a database\*\*: the assignment didn't require persistence, and adding one (connection strings, migrations, Docker Compose) would add setup time disproportionate to a scoped exercise. The interface-based design makes this a deliberate, reversible choice rather than a limitation.

\- \*\*API key in configuration, not hardcoded\*\*: for local development it lives in `appsettings.Development.json` (excluded from source control). For the Docker run, it's passed as an environment variable — in a real production deployment, this would come from a secrets manager (e.g. Azure Key Vault) rather than either of these.

\- \*\*No CQRS / MediatR / Clean Architecture\*\*: appropriate for a large, long-lived system, but unnecessary ceremony for a service this size. Three well-separated layers (middleware, controllers, services) are enough structure to keep responsibilities clear without over-engineering a two-hour exercise.



\## Known Limitations



\- The CSV parser handles standard comma-separated files but does not support commas embedded inside quoted fields spanning multiple lines. A library like CsvHelper would be the production-grade choice.

\- Tracking history is not persisted and resets on restart or container recreation.

\- A single, static API key is used — no per-client keys, rotation, or scopes.



\## Testing



Manually verified via Swagger UI:

\- Successful upload with auto-detected column and with an explicitly specified column

\- Rejected upload for a non-existent column name (400)

\- Rejected upload for a missing file (400)

\- Rejected request with a missing or invalid API key (401)

\- Reporting endpoints reflect uploads and failures accurately after multiple test runs

