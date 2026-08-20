# Flight Schedule Maintenance

A full-stack flight schedule management application built with **Angular 22**, **ASP.NET Core Web API (.NET 8)**, **Microsoft SQL Server**, and **raw ADO.NET**.

The application allows internal airline staff to manage published flight schedules, including creating, editing, filtering, changing status, and deleting schedules.

---

## Features

- View all flight schedules
- Filter schedules by:
  - Origin airport
  - Destination airport
  - Status
- Create new flight schedules
- Edit existing schedules
- Change schedule status directly from the schedule list
- Delete schedules with confirmation
- Display origin and destination using IATA code and city
- Display `+1` for next-day arrivals
- Support open-ended effective periods
- Validate schedule data in both frontend and backend
- Return clear API errors for validation, missing schedules, and duplicate schedules
- Loading and empty-result states in the Angular UI

---

## Technology Stack

### Frontend

- Angular 22
- TypeScript
- Standalone components
- Angular Router
- Angular HttpClient
- Typed Reactive Forms
- Angular Signals
- Plain CSS

### Backend

- ASP.NET Core Web API
- .NET 8
- C#
- Raw ADO.NET
- `Microsoft.Data.SqlClient`
- Swagger / OpenAPI

### Database

- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

### Version Control

- Git
- GitHub

---

## Project Structure

```text
FlightScheduleMaintenance/
│
├── backend/
│   └── ScheduleManagement.Api/
│       ├── Common/
│       ├── Controllers/
│       ├── Models/
│       │   ├── Entities/
│       │   ├── Requests/
│       │   └── Responses/
│       ├── Repositories/
│       ├── Services/
│       ├── Properties/
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Program.cs
│       └── ScheduleManagement.Api.csproj
│
├── database/
│   └── schema.sql
│
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   │   ├── models/
│   │   │   │   └── services/
│   │   │   └── features/
│   │   │       └── schedules/
│   │   │           ├── pages/
│   │   │           └── validators/
│   │   └── environments/
│   ├── angular.json
│   ├── package.json
│   └── package-lock.json
│
└── README.md
```

---

## Prerequisites

Install the following before running the project:

- Node.js 22 LTS or later
- npm
- Angular CLI 22.x
- .NET SDK 8.0
- Microsoft SQL Server Developer or Express
- SQL Server Management Studio
- Git

Verify the main tools:

```powershell
node --version
npm --version
ng version
dotnet --version
git --version
```

---

## Database Setup

1. Open SQL Server Management Studio.
2. Connect to your SQL Server instance.
3. Create a database named:

```text
FlightScheduleDb
```

4. Open:

```text
database/schema.sql
```

5. Run the script against `FlightScheduleDb`.

The script creates the required tables, constraints, foreign keys, and airport seed data.

### Main Tables

#### Airport

Stores airport reference data.

Important fields include:

- `AirportId`
- `IataCode`
- `AirportName`
- `City`
- `CountryCode`
- `IsActive`

#### FlightSchedule

Stores the maintained flight schedules.

Important fields include:

- `ScheduleId`
- `FlightNumber`
- `OriginAirportId`
- `DestinationAirportId`
- `DepartureTime`
- `ArrivalTime`
- `AircraftType`
- `DaysOfOperation`
- `EffectiveFrom`
- `EffectiveTo`
- `Status`
- `CreatedOn`
- `ModifiedOn`

---

## Backend Configuration

Navigate to:

```text
backend/ScheduleManagement.Api
```

Configure the SQL Server connection string in `appsettings.json` or the development configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESSCUSTOM;Database=FlightScheduleDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Change the SQL Server instance name if your local setup is different.

---

## Run the Backend

From the project root:

```powershell
cd backend\ScheduleManagement.Api
dotnet restore
dotnet build
dotnet run --launch-profile https
```

The development API is configured to run on:

```text
https://localhost:7178
```

Swagger:

```text
https://localhost:7178/swagger
```

The HTTP launch URL may also be available depending on the selected launch profile.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/airports` | Get active airports |
| GET | `/api/schedules` | Get schedules with optional filters |
| GET | `/api/schedules/{id}` | Get one schedule |
| POST | `/api/schedules` | Create a schedule |
| PUT | `/api/schedules/{id}` | Update a schedule |
| PATCH | `/api/schedules/{id}/status` | Update schedule status |
| DELETE | `/api/schedules/{id}` | Delete a schedule |

### Schedule Filters

`GET /api/schedules` supports:

```text
origin
destination
status
```

Example:

```text
GET /api/schedules?origin=1&destination=5&status=Published
```

Filters are processed by the backend and applied in SQL.

---

## API Response Codes

Common response codes include:

- `200 OK` - successful request
- `201 Created` - schedule created successfully
- `204 No Content` - schedule deleted successfully
- `400 Bad Request` - validation failure
- `404 Not Found` - schedule does not exist
- `409 Conflict` - duplicate flight number and effective-from date

---

## Frontend Setup

Navigate to the Angular project:

```powershell
cd frontend
```

Install dependencies:

```powershell
npm install
```

The Angular application uses environment files for the backend API URL.

Development environment example:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7178/api'
};
```

---

## Run the Frontend

From the `frontend` folder:

```powershell
ng serve --prebundle=false
```

Then open:

```text
http://localhost:4200
```

`--prebundle=false` can be used in this development environment to avoid local Windows/Vite dependency-cache locking issues.

---

## Running Frontend and Backend Together

Use two terminals.

### Terminal 1 - Backend

```powershell
cd backend\ScheduleManagement.Api
dotnet run --launch-profile https
```

### Terminal 2 - Frontend

```powershell
cd frontend
ng serve --prebundle=false
```

Open:

```text
http://localhost:4200
```

---

## CORS

The backend allows requests from the Angular development origin:

```text
http://localhost:4200
```

The application should not require `AllowAnyOrigin()` for local development.

---

## Schedule Validation Rules

The application validates schedule data in both Angular and the API.

### Flight Number

Format:

```text
Two letters + 3 or 4 digits
```

Examples:

```text
UL308
UL503
```

### Airports

- Origin is required
- Destination is required
- Origin and destination must be different
- Airport IDs must reference existing airports

### Times

- Departure time is required
- Arrival time is required
- Arrival earlier than departure is valid and represents a next-day arrival

Example:

```text
Departure: 23:30
Arrival:   05:30
```

The list displays the arrival as next day using `+1`.

### Aircraft Type

Allowed values:

```text
A320
A330
A350
```

### Days of Operation

A seven-character value represents Monday through Sunday.

Examples:

```text
1234567
12.4.6.
1.3.5..
```

A dot means the flight does not operate on that day.

At least one operating day must be selected.

### Effective Dates

- `EffectiveFrom` is required
- `EffectiveTo` can be empty for an open-ended schedule
- If supplied, `EffectiveTo` cannot be earlier than `EffectiveFrom`

### Status

Allowed values:

```text
Draft
Published
Suspended
```

---

## Duplicate Schedule Rule

The database enforces uniqueness for:

```text
FlightNumber + EffectiveFrom
```

Submitting a duplicate combination returns:

```text
409 Conflict
```

---

## Backend Architecture

The backend follows a layered structure:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
ADO.NET
    ↓
SQL Server
```

### Controllers

Responsible for:

- HTTP requests
- HTTP responses
- Request-level handling
- Mapping API responses

### Services

Responsible for:

- Business rules
- Schedule validation
- Status validation
- Coordinating repository operations

### Repositories

Responsible for:

- Raw SQL
- `SqlConnection`
- `SqlCommand`
- `SqlDataReader`
- Parameterized database access

The project does not use Entity Framework, Dapper, or another ORM.

---

## SQL Security

All caller-provided values are passed to SQL using parameters.

Example:

```csharp
command.Parameters.Add(
    "@AirportId",
    SqlDbType.Int
).Value = airportId;
```

The project avoids:

- SQL string interpolation with user input
- `AddWithValue`
- `SELECT *`

---

## Angular Architecture

The frontend uses modern Angular patterns:

- Standalone components
- `inject()`
- Signals
- `@if`
- `@for`
- Typed Reactive Forms
- `FormBuilder`
- Group-level cross-field validators
- Lazy `loadComponent()` routing
- `provideHttpClient()`
- `provideRouter()`
- Environment-based API URL

---

## Main Frontend Routes

```text
/schedules
/schedules/new
/schedules/:id/edit
```

### `/schedules`

Provides:

- Schedule table
- Origin filter
- Destination filter
- Status filter
- Inline status update
- Edit action
- Delete action
- Loading state
- Empty state

### `/schedules/new`

Creates a new schedule.

### `/schedules/:id/edit`

Loads an existing schedule and updates the same record.

---

## Build Verification

### Backend

```powershell
cd backend\ScheduleManagement.Api
dotnet clean
dotnet build
```

Expected:

```text
Build succeeded.
0 Error(s)
```

### Frontend

```powershell
cd frontend
ng build
```

Expected:

```text
Application bundle generation complete.
```

---

## Git

Check the repository before pushing:

```powershell
git status
```

Commit example:

```powershell
git add .
git commit -m "docs: add project setup and usage guide"
git push
```

Do not commit generated dependencies or build output such as:

```text
node_modules/
.angular/cache/
dist/
backend/**/bin/
backend/**/obj/
```

---

## Development Notes

- The `Airport` table is reference data for this module.
- Only active airports are returned by `/api/airports`.
- Schedule filtering is performed in SQL rather than in the browser.
- The schedule list resolves origin and destination using airport joins.
- `CreatedOn` is set when a schedule is created.
- `ModifiedOn` is updated when a schedule is modified.
- Authentication, audit logging, soft delete, timezone conversion, and automated tests are outside the current project scope.

---

## License

This project was created for internship learning and technical evaluation purposes.
