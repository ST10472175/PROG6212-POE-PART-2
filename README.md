# RaceDay API (PROG6212 Part 2)

RESTful ASP.NET Core Web API for the RaceDay system. Organisers create running, walking and cycling events and define categories. Participants enter events by choosing a category and later view their results.

**Technology:** ASP.NET Core 8 Web API, Entity Framework Core (Code-First), SQL Server, server-side sessions, BCrypt password hashing, Swagger, xUnit and GitHub Actions.

## Folder structure

```
RaceDay/
├── RaceDay.sln
├── README.md
├── docs/green-build.png            CI screenshot
├── .github/workflows/dotnet.yml    GitHub Actions: structure check, build and unit tests
├── RaceDay.Api/
│   ├── Program.cs                  services, session, Swagger and pipeline
│   ├── appsettings.json            connection string and organiser code
│   ├── Controllers/                Auth, Profile, Events, Categories, Enrolments, Results
│   ├── Models/                     database tables (EF Core entities)
│   ├── Dtos/                       data sent to and from the API
│   └── Data/RaceDayContext.cs      DbContext, keys and relationships
└── RaceDay.Tests/                  xUnit tests (auth, events, role access, enrolments, results)
```

## Setup and run

**You need:** Visual Studio 2022, the .NET 8 SDK and SQL Server (LocalDB is installed with Visual Studio).

1. Clone the repository and open `RaceDay.sln` in Visual Studio.
2. Open `RaceDay.Api/appsettings.json` and check `ConnectionStrings:DefaultConnection`. The default uses LocalDB and creates a database called `RaceDayDb`.
3. Create the database tables from the code (Code-First). Open **Tools > NuGet Package Manager > Package Manager Console**, set the **Default project** to `RaceDay.Api` and run:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
4. Right click `RaceDay.Api`, choose **Set as Startup Project**, and press **F5**. The browser opens on the Swagger page (`/swagger`).
5. To run the unit tests, open **Test > Test Explorer** and click **Run All Tests**, or run `dotnet test` in the solution folder. The tests use an in-memory database, so no SQL Server is needed.

### How to test the API in Swagger
1. Open `POST /api/auth/register`, click **Try it out**, fill in the details and click **Execute**.
2. Open `POST /api/auth/login` and log in with the same email and password. The server creates a session and the browser keeps the session cookie, so every other endpoint on the page now works as that user.
3. Try the other endpoints. To test the other role, call `POST /api/auth/logout` and log in as a different user.

## The two roles

| Role | What they can do |
|------|------------------|
| **Organiser** | Create, update and delete their own events. Create, rename and delete categories for their events. View all enrolments for their events. Capture and correct finish times and finishing positions. |
| **Participant** | View events and categories. Enter an event by choosing a category. View their own enrolments. View their own results. |

Both roles can register, log in, log out, view events and categories, and view and update their own profile.

### How the role is managed when registering
If anyone could simply pick "Organiser", any user could take over event management. Registering as an **Organiser** therefore needs a secret organiser code (`Registration:OrganiserCode` in `appsettings.json`, default `RACEDAY-ORGANISER-2026`, change it before deploying). Registering as a **Participant** needs no code. The role is saved on the user's account, and at login it is stored in the server-side session, so it is never taken from the request.

## Authentication and security
- **Hashed passwords:** passwords are hashed with BCrypt before they are saved. The original password is never stored.
- **Server-side session:** when a user logs in, the server stores the user id and role in a session and gives the browser an HttpOnly session cookie (30 minute idle timeout). Every later request is checked against that session.
- **Role enforcement:** every endpoint except register and login needs a logged in user (401 if not). Organiser endpoints return 403 to Participants and Participant endpoints return 403 to Organisers. An Organiser can only change events that belong to them.

## Endpoints

| Area | Endpoint | Who can use it |
|------|----------|----------------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` | Anyone |
| Auth | `POST /api/auth/logout` | Logged in users |
| Profile | `GET /api/profile`, `PUT /api/profile` | Both roles (own profile) |
| Events | `GET /api/events`, `GET /api/events/{id}` | Both roles |
| Events | `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` | Organiser (own events) |
| Categories | `GET /api/events/{eventId}/categories` | Both roles |
| Categories | `POST /api/events/{eventId}/categories`, `PUT /api/categories/{categoryId}`, `DELETE /api/categories/{categoryId}` | Organiser (own events) |
| Enrolments | `POST /api/events/{eventId}/enrolments`, `GET /api/enrolments/my` | Participant |
| Enrolments | `GET /api/events/{eventId}/enrolments` | Organiser (own events) |
| Results | `POST /api/events/{eventId}/results`, `GET /api/events/{eventId}/results`, `PUT /api/results/{resultsId}` | Organiser (own events) |
| Results | `GET /api/results/my` | Participant |

Swagger shows the purpose, expected input and possible responses of every endpoint.

## Database (matches the Part 1 ERD)
Tables: `EventOrganiser`, `Participant`, `Events`, `Category`, `Assignment` (the enrolment table) and `Results`, with the same columns and relationships as the Part 1 ERD. These additions were needed for the Part 2 requirements:
- `Users` table: email, password hash and role, linked to one organiser or one participant. This is what login uses.
- `Events.Description`, `Events.Distance` (km) and `Events.OrganiserId` (the "create" relationship between an organiser and their events).
- `Assignment.CategoryId`, because an enrolment must record the selected category.
- Unique rules: a participant can enter an event only once and has only one result per event.

## Unit tests
The xUnit tests start the real API in memory with an in-memory database and cover both success and failure cases:
- **Authentication:** registration (both roles, organiser code, duplicate email, invalid data), hashed passwords, login, wrong password, session, logout and profile.
- **Role-based access:** every Organiser endpoint rejects Participants, every Participant endpoint rejects Organisers and logged out users are rejected everywhere.
- **Events and categories:** create, view, update and delete, ownership rules, and category rules.
- **Enrolments:** enrolling, the recorded link between participant, event and category, duplicate enrolment, and viewing enrolments.
- **Results:** capturing, validation, duplicate position, correcting, and each participant seeing only their own results.

## GitHub and CI/CD
The workflow `.github/workflows/dotnet.yml` runs on every push and pull request. It checks that the required files and folders exist, builds the solution and runs all the unit tests.

![Green GitHub Actions build](docs/green-build.png)

## Demo video
YouTube (unlisted): _paste the link here_
