# StuffTracker — Implementationsplan v3

> **Senast uppdaterad: 2026-03-25**
> Ersätter `2026-03-20-stufftracker-implementation.md`.
>
> **Ändringar från v2:**
> - `Position` borttaget ur `LocationType`. Täcks av `Storage → Storage`-nesting.
>   Hierarkireglerna är nu: Room → parent måste vara Home. Storage → parent måste vara Room eller Storage.

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bygg en komplett backend för StuffTracker — en app där användare organiserar sina ägodelar i en platshierarki och kan söka efter dem.

**Architecture:** Clean Architecture med CQRS (MediatR), vertikal slice-struktur. Varje skiva går genom Domain → Application → Infrastructure → API. Tracer-bullet-approach: bygg tunt, verifiera, iterera.

**Tech Stack:** .NET 10, ASP.NET Core Web API, EF Core + SQL Server (LocalDB), MediatR, FluentValidation, AutoMapper, ASP.NET Core Identity + JWT, Serilog.

---

## Nuläge

Skiva 1 (projektstruktur + CreateHome/GetHome) är delvis implementerad:
- Clean Architecture-skelett med fyra projekt
- Location-entitet med self-referencing FK och LocationType-enum
- CreateHomeCommand + GetHomeByIdQuery via MediatR
- EF Core DbContext, migration, LocationsRepository
- LocationController med POST + GET

**Saknas från skiva 1:**
- Unsorted-nod skapas inte automatiskt vid hem-skapande
- Swagger/OpenAPI saknas
- Felhantering använder generisk `Exception`
- Max-längd-konstanter är duplicerade (validator vs EF-config)
- Inget globalt query-filter för soft delete

---

## Skiva 0 — Stabilisera grunden

**Mål:** Fixa det som saknas från skiva 1 så att grunden är solid innan du bygger vidare.

**Files:**
- Modify: `src/StuffTracker.API/Program.cs`
- Modify: `src/StuffTracker.Application/Locations/Commands/CreateHome/CreateHomeCommandHandler.cs`
- Modify: `src/StuffTracker.Application/Locations/Queries/GetHomeById/GetHomeByIdQueryHandler.cs`
- Modify: `src/StuffTracker.Infrastructure/Persistance/StuffTrackerDbContext.cs`
- Modify: `src/StuffTracker.Domain/Entities/Location.cs`
- Modify: `src/StuffTracker.Infrastructure/Configuration/LocationConfiguration.cs`
- Create: `src/StuffTracker.Domain/Exceptions/NotFoundException.cs`
- Create: `src/StuffTracker.Domain/Exceptions/BusinessRuleException.cs`
- Create: `src/StuffTracker.Domain/Constants/LocationConstants.cs`
- Create: `src/StuffTracker.Application/Common/Behaviors/ValidationBehavior.cs`
- Create: `src/StuffTracker.API/Controllers/HomesController.cs`
- Delete: `src/StuffTracker.API/WeatherForecast.cs`

### 0.1 — Städa + Swagger/Scalar

- [ ] Ta bort `WeatherForecast.cs` från API-projektet (oanvänd template-fil)
- [ ] Fixa namespace-mismatch: mappen heter `GetHomeById` men namespacet är `GetHome` — välj ett och var konsekvent
- [ ] Installera `Microsoft.AspNetCore.OpenApi` och `Scalar.AspNetCore` i API-projektet
- [ ] Konfigurera OpenAPI i `Program.cs` med `builder.Services.AddOpenApi()` och `app.MapOpenApi()`
- [ ] Lägg till Scalar UI med `app.MapScalarApiReference()`
- [ ] Kör API:t, öppna `/scalar/v1` och verifiera att du ser dina endpoints
- [ ] Commit: `feat: add OpenAPI with Scalar UI`

### 0.2 — Flytta CreateHome till rätt route + HomeId-denormalisering

Nuvarande `POST /api/locations` för CreateHome kommer att kollidera med CreateLocation i skiva 1. Lös detta nu.

- [ ] Skapa `HomesController` med route `api/homes`
- [ ] Flytta CreateHome-endpoint dit: `POST /api/homes`
- [ ] Flytta GetHomeById dit: `GET /api/homes/{id}`
- [ ] Lägg till `GET /api/homes` — en ny `GetHomesQuery` som returnerar alla hem (för nu utan auth-filtrering, det kommer i skiva 3)
- [ ] **HomeId-denormalisering:** Lägg till `HomeId` (nullable Guid, FK) på `Location`-entiteten. För Home-noder sätts HomeId till sitt eget Id. För alla child-locations kopieras parent.HomeId. Detta förenklar alla framtida queries drastiskt.
- [ ] Uppdatera `LocationConfiguration` med FK + index för HomeId
- [ ] Skapa ny migration
- [ ] Testa: skapa hem, verifiera att HomeId sätts korrekt
- [ ] Commit: `refactor: separate HomesController, add HomeId denormalization`

### 0.3 — Skapa Unsorted-nod vid hem-skapande

- [ ] Uppdatera `CreateHomeCommandHandler` så att den efter att hemmet skapats även skapar en Location med `LocationType.Unsorted`, `Name = "Unsorted"`, `ParentId = homeId`, `HomeId = homeId`
- [ ] Se till att båda sparas i samma transaktion — anropa `SaveChangesAsync()` en enda gång efter alla ändringar
- [ ] Testa via Scalar: skapa ett hem, hämta det, och verifiera att Unsorted-noden finns
- [ ] Commit: `feat: auto-create Unsorted node when creating home`

### 0.4 — Custom exceptions + globalt query-filter

- [ ] Skapa `NotFoundException` i Domain (ärver från `Exception`, tar entitynamn + id)
- [ ] Skapa `BusinessRuleException` i Domain (för affärsregelbrott, t.ex. ogiltiga hierarki-kombinationer)
- [ ] Ersätt generiska `throw new Exception()` i `GetHomeByIdQueryHandler` med `NotFoundException`
- [ ] Lägg till globalt query-filter i `StuffTrackerDbContext.OnModelCreating`:
  ```csharp
  modelBuilder.Entity<Location>().HasQueryFilter(l => !l.IsDeleted);
  ```
- [ ] Ta bort manuellt `!IsDeleted`-filter från `LocationsRepository.GetLocationById`
- [ ] Testa: hämta ett hem som inte finns → bör få ett tydligt felmeddelande (500 för nu, middleware fixas i skiva 7)
- [ ] Commit: `refactor: add custom exceptions and global soft-delete filter`

### 0.5 — Extrahera konstanter (DRY)

- [ ] Skapa `LocationConstants.cs` i Domain med max-längder (`NameMaxLength = 200`, `DescriptionMaxLength = 500`)
- [ ] Uppdatera `CreateHomeCommandValidator` att använda konstanterna
- [ ] Uppdatera `LocationConfiguration` att använda konstanterna
- [ ] Verifiera att API:t fortfarande fungerar
- [ ] Commit: `refactor: extract Location field length constants`

### 0.6 — ValidationBehavior (så att validators faktiskt körs)

FluentValidation-validators registreras men anropas aldrig automatiskt utan en MediatR pipeline behavior. Fixa detta nu så att alla validators fungerar från start.

- [ ] Skapa `ValidationBehavior<TRequest, TResponse>` i `src/StuffTracker.Application/Common/Behaviors/`
  - Injicera `IEnumerable<IValidator<TRequest>>`
  - Kör alla validators, samla fel, kasta `ValidationException` om det finns fel
- [ ] Registrera som `IPipelineBehavior` i `ServiceCollectionExtensions.AddApplication()`
- [ ] Testa: skicka ett CreateHome-request med tomt Name → bör ge valideringsfel
- [ ] Commit: `feat: add MediatR ValidationBehavior pipeline`

---

## Skiva 1 — Skapa platser i hierarkin

**Mål:** Kunna skapa Room och Storage under ett Home. Validera att hierarkin följer reglerna.

> **LocationType-regler (uppdaterade 2026-03-25):**
> - `Room` → parent måste vara `Home`
> - `Storage` → parent måste vara `Room` eller `Storage`
> - `Home` och `Unsorted` accepteras inte av denna endpoint
> - `Position` finns inte längre — nested `Storage` täcker det use caset

**Files:**
- Create: `src/StuffTracker.Application/Locations/Commands/CreateLocation/CreateLocationCommand.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/CreateLocation/CreateLocationCommandHandler.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/CreateLocation/CreateLocationCommandValidator.cs`
- Create: `src/StuffTracker.Application/Locations/Dtos/LocationDto.cs`
- Modify: `src/StuffTracker.Application/Locations/Dtos/HomesProfile.cs` (rename to `LocationsProfile.cs`)
- Modify: `src/StuffTracker.Domain/Repositories/ILocationsRepository.cs`
- Modify: `src/StuffTracker.Infrastructure/Repositories/LocationsRepository.cs`
- Modify: `src/StuffTracker.API/Controllers/LocationController.cs`

### 1.1 — CreateLocation command + validering

- [ ] Skapa `CreateLocationCommand` med properties: `Name`, `Description`, `LocationType`, `ParentId`
- [ ] Skapa validator med regler:
  - Name: required, max längd
  - LocationType: måste vara `Room` eller `Storage` (inte `Home`, `Unsorted`)
  - ParentId: required (alla icke-Home-platser måste ha en förälder)
- [ ] Commit: `feat: add CreateLocation command with validation`

### 1.2 — Hierarkivalidering i handler

Hierarkivalidering kräver DB-uppslag (vi måste hämta parent för att kontrollera dess typ). Valideringen placeras därför i handlern, inte i FluentValidation-validatorn.

- [ ] Skapa `CreateLocationCommandHandler`
- [ ] Hämta parent-location från repository (kasta `NotFoundException` om den inte finns)
- [ ] Validera tillåtna parent-child-kombinationer — kasta `BusinessRuleException` vid brott:
  - `Room` → parent måste vara `Home`
  - `Storage` → parent måste vara `Room` eller `Storage`
- [ ] Kopiera `HomeId` från parent automatiskt
- [ ] Skapa `LocationDto` (Id, Name, Description, LocationType, ParentId, HomeId, CreatedAt)
- [ ] Lägg till mappnings-profil
- [ ] Commit: `feat: add hierarchy validation for location creation`

### 1.3 — Controller-endpoint + testa

- [ ] Lägg till `POST /api/homes/{homeId}/locations` i `LocationController` (route: `api/homes/{homeId}/locations`)
- [ ] Testa hela kedjan i Scalar:
  1. Skapa ett hem via `POST /api/homes` → får tillbaka homeId
  2. Skapa ett rum under hemmet (LocationType=Room, ParentId=homeId)
  3. Skapa en förvaringsplats under rummet (LocationType=Storage)
  4. Skapa en nästlad förvaringsplats under den (LocationType=Storage) — t.ex. en låda i en garderob
  5. Försök skapa ett rum under en Storage → bör ge 422
  6. Försök skapa en Storage direkt under Home → bör ge 422
- [ ] Commit: `feat: add POST endpoint for creating locations`

---

## Skiva 2 — Hämta, uppdatera och ta bort platser

**Mål:** Komplett CRUD för Location-hierarkin.

**Files:**
- Create: `src/StuffTracker.Application/Locations/Queries/GetLocationsByHome/GetLocationsByHomeQuery.cs`
- Create: `src/StuffTracker.Application/Locations/Queries/GetLocationsByHome/GetLocationsByHomeQueryHandler.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/UpdateLocation/UpdateLocationCommand.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/UpdateLocation/UpdateLocationCommandHandler.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/DeleteLocation/DeleteLocationCommand.cs`
- Create: `src/StuffTracker.Application/Locations/Commands/DeleteLocation/DeleteLocationCommandHandler.cs`
- Modify: `src/StuffTracker.Domain/Repositories/ILocationsRepository.cs`
- Modify: `src/StuffTracker.Infrastructure/Repositories/LocationsRepository.cs`
- Modify: `src/StuffTracker.API/Controllers/LocationController.cs`

### 2.1 — GetLocationsByHome query

- [ ] Skapa query + handler som hämtar alla platser för ett hem
- [ ] Repositorymetod: tack vare HomeId-denormaliseringen i 0.2 blir detta en enkel `WHERE HomeId = @homeId` — ingen rekursiv traversering behövs
- [ ] Returnera flat lista med LocationDto
- [ ] Lägg till `GET /api/homes/{homeId}/locations` i controllern (konsekvent med `POST /api/homes/{homeId}/locations` från skiva 1)
- [ ] Testa: skapa hierarki, hämta alla platser, verifiera att rätt platser returneras
- [ ] Commit: `feat: add GetLocationsByHome query`

### 2.2 — UpdateLocation command

- [ ] Skapa command med: `Id`, `Name`, `Description` — detta är en **PUT**-semantik (ersätter alla redigerbara fält)
- [ ] Handler: hämta location, uppdatera fält, spara. Alla fält i commandet skrivs (Name krävs, Description kan vara null för att rensa)
- [ ] Validering: Name required, max längd. Unsorted-nod kan inte uppdateras.
- [ ] Repositorymetod för att spara ändringar
- [ ] Lägg till `PUT /api/locations/{id}` i controllern
- [ ] Testa: skapa plats, uppdatera namn, hämta igen
- [ ] Commit: `feat: add UpdateLocation command`

### 2.3 — DeleteLocation command (soft delete)

- [ ] Skapa command med: `Id`
- [ ] Handler-logik:
  - Hämta location
  - Unsorted-noden kan **inte** tas bort (kasta exception)
  - Home kan **inte** tas bort via denna command (separat DeleteHome i framtiden)
  - Sätt `IsDeleted = true`, `DeletedAt = DateTime.UtcNow`
  - **Items-hantering:** Flytta alla items i den borttagna platsen till Unsorted-noden (implementeras fullt i skiva 5, men förbered strukturen nu)
  - Soft-deleta även alla child-locations rekursivt
- [ ] Lägg till `DELETE /api/locations/{id}` i controllern
- [ ] Testa: skapa hierarki, ta bort en förvaringsplats, verifiera att den inte längre syns i GetLocationsByHome
- [ ] Commit: `feat: add soft delete for locations`

---

## Skiva 3 — Autentisering (Identity + JWT)

**Mål:** Riktiga användare som kan registrera sig och logga in. Alla endpoints kräver token.

**Files:**
- Create: `src/StuffTracker.Domain/Entities/User.cs`
- Create: `src/StuffTracker.Domain/Entities/UserHome.cs`
- Create: `src/StuffTracker.Application/Auth/Commands/Register/RegisterCommand.cs`
- Create: `src/StuffTracker.Application/Auth/Commands/Register/RegisterCommandHandler.cs`
- Create: `src/StuffTracker.Application/Auth/Commands/Login/LoginCommand.cs`
- Create: `src/StuffTracker.Application/Auth/Commands/Login/LoginCommandHandler.cs`
- Create: `src/StuffTracker.Application/Auth/Dtos/AuthResponseDto.cs`
- Create: `src/StuffTracker.Application/Users/IUserContext.cs`
- Create: `src/StuffTracker.Infrastructure/Auth/UserContext.cs`
- Create: `src/StuffTracker.Infrastructure/Auth/JwtTokenService.cs`
- Create: `src/StuffTracker.Infrastructure/Configuration/UserHomeConfiguration.cs`
- Modify: `src/StuffTracker.Infrastructure/Persistance/StuffTrackerDbContext.cs`
- Modify: `src/StuffTracker.Infrastructure/Extensions/ServiceCollectionExtension.cs`
- Modify: `src/StuffTracker.API/Program.cs`
- Modify: `src/StuffTracker.API/Controllers/LocationController.cs`
- Create: `src/StuffTracker.API/Controllers/AuthController.cs`

### 3.1 — User-entitet och Identity-setup

- [ ] Skapa `User` som ärver `IdentityUser` i Domain
- [ ] Konfigurera Identity i Infrastructure: `AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<StuffTrackerDbContext>()`
- [ ] Uppdatera DbContext att ärva från `IdentityDbContext<User>` istället för `DbContext`
- [ ] Skapa migration
- [ ] Verifiera att API:t startar utan fel
- [ ] Commit: `feat: add User entity with ASP.NET Core Identity`

### 3.2 — Register-endpoint

- [ ] Skapa `RegisterCommand` (Email, Password, DisplayName)
- [ ] Handler: använd `UserManager<User>` för att skapa användare
- [ ] Skapa `AuthController` med `POST /api/auth/register`
- [ ] Testa via Scalar: registrera en användare
- [ ] Commit: `feat: add user registration endpoint`

### 3.3 — Login-endpoint med JWT

- [ ] Installera `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] Lägg till JWT-konfiguration i `appsettings.Development.json`:
  ```json
  "Jwt": {
    "Issuer": "StuffTracker",
    "Audience": "StuffTracker",
    "Key": "din-development-hemliga-nyckel-minst-32-tecken"
  }
  ```
- [ ] Skapa `JwtTokenService` som genererar JWT-token med claims (UserId, Email)
- [ ] Skapa `LoginCommand` + handler: validera credentials med UserManager, returnera JWT
- [ ] Konfigurera JWT Bearer auth i `Program.cs` (läs från `Configuration["Jwt:Key"]` etc.)
- [ ] Lägg till `POST /api/auth/login` i AuthController
- [ ] Testa: registrera → logga in → få JWT-token
- [ ] Commit: `feat: add login endpoint with JWT token generation`

### 3.4 — Skydda endpoints + UserHome-koppling

- [ ] Lägg till `[Authorize]` på `HomesController` och `LocationController`
- [ ] Skapa `IUserContext`-interface i Application med property `UserId`
- [ ] Implementera `UserContext` i Infrastructure (läs UserId från HttpContext.User claims)
- [ ] Skapa `UserHome`-entitet (UserId, HomeId, Role enum: Owner/Member)
- [ ] Konfigurera i DbContext + skapa migration
- [ ] Uppdatera dessa handlers att använda `IUserContext` och filtrera via `UserHome`:
  - `CreateHomeCommandHandler` — skapa `UserHome`-rad (Role = Owner) när hem skapas
  - `GetHomesQueryHandler` — returnera bara hem där användaren har UserHome-koppling
  - `GetHomeByIdQueryHandler` — verifiera att användaren äger hemmet
  - `CreateLocationCommandHandler` — verifiera att parent-location tillhör användarens hem
  - `GetLocationsByHomeQueryHandler` — verifiera hem-ägarskap
  - `UpdateLocationCommandHandler` — verifiera hem-ägarskap
  - `DeleteLocationCommandHandler` — verifiera hem-ägarskap
- [ ] Testa: logga in som User A, skapa hem. Logga in som User B → ser inte User A:s hem
- [ ] Commit: `feat: add authorization with UserHome ownership`

### 3.5 — Swagger auth-stöd

- [ ] Konfigurera Scalar/OpenAPI att visa "Authorize"-knapp för JWT Bearer
- [ ] Testa: logga in, kopiera token, auktorisera i Scalar, anropa skyddade endpoints
- [ ] Commit: `feat: add JWT auth support in Scalar UI`

---

## Skiva 4 — Categories

**Mål:** Kategorier (globala + hemspecifika) som Items kan kopplas till.

**Files:**
- Create: `src/StuffTracker.Domain/Entities/Category.cs`
- Create: `src/StuffTracker.Domain/Repositories/ICategoriesRepository.cs`
- Create: `src/StuffTracker.Application/Categories/Commands/CreateCategory/CreateCategoryCommand.cs`
- Create: `src/StuffTracker.Application/Categories/Commands/CreateCategory/CreateCategoryCommandHandler.cs`
- Create: `src/StuffTracker.Application/Categories/Queries/GetCategories/GetCategoriesQuery.cs`
- Create: `src/StuffTracker.Application/Categories/Queries/GetCategories/GetCategoriesQueryHandler.cs`
- Create: `src/StuffTracker.Application/Categories/Dtos/CategoryDto.cs`
- Create: `src/StuffTracker.Infrastructure/Configuration/CategoryConfiguration.cs`
- Create: `src/StuffTracker.Infrastructure/Repositories/CategoriesRepository.cs`
- Create: `src/StuffTracker.Infrastructure/Seeders/CategorySeeder.cs`
- Create: `src/StuffTracker.API/Controllers/CategoriesController.cs`
- Modify: `src/StuffTracker.Infrastructure/Persistance/StuffTrackerDbContext.cs`

### 4.1 — Category-entitet + seed data

- [ ] Skapa `Category`-entitet: Id, Name, HomeId (nullable — null = global), IsDeleted
- [ ] EF-konfiguration + migration
- [ ] Seed globala kategorier: "Tools", "Electronics", "Documents", "Kitchen", "Clothing" (via `HasData` i configuration eller en seeder-klass)
- [ ] Kör migration, verifiera att globala kategorier finns i databasen
- [ ] Commit: `feat: add Category entity with global seed data`

### 4.2 — Category CRUD

- [ ] `CreateCategory` command — skapar hemspecifik kategori (HomeId krävs, verifiera att användaren äger hemmet)
- [ ] `GetCategories` query — returnerar globala (HomeId = null) + hemspecifika för ett givet hem
- [ ] `UpdateCategory` command — bara hemspecifika kategorier kan uppdateras (globala är skyddade)
- [ ] `DeleteCategory` command — soft delete, bara hemspecifika (globala kan inte tas bort)
- [ ] `CategoriesController` med:
  - `POST /api/homes/{homeId}/categories`
  - `GET /api/homes/{homeId}/categories`
  - `PUT /api/categories/{id}`
  - `DELETE /api/categories/{id}`
- [ ] Testa: skapa hemspecifik kategori, hämta alla (ser globala + hemspecifika), uppdatera, ta bort. Försök ändra global kategori → bör ge fel.
- [ ] Commit: `feat: add Category CRUD endpoints`

---

## Skiva 5 — Items (CRUD)

**Mål:** Saker kan skapas, kopplas till platser och kategorier, filtreras och pagineras.

**Files:**
- Create: `src/StuffTracker.Domain/Entities/Item.cs`
- Create: `src/StuffTracker.Domain/Enums/ItemStatus.cs`
- Create: `src/StuffTracker.Domain/Repositories/IItemsRepository.cs`
- Create: `src/StuffTracker.Application/Items/Commands/CreateItem/CreateItemCommand.cs`
- Create: `src/StuffTracker.Application/Items/Commands/CreateItem/CreateItemCommandHandler.cs`
- Create: `src/StuffTracker.Application/Items/Commands/MoveItem/MoveItemCommand.cs`
- Create: `src/StuffTracker.Application/Items/Commands/MoveItem/MoveItemCommandHandler.cs`
- Create: `src/StuffTracker.Application/Items/Commands/UpdateItem/UpdateItemCommand.cs`
- Create: `src/StuffTracker.Application/Items/Commands/DeleteItem/DeleteItemCommand.cs`
- Create: `src/StuffTracker.Application/Items/Queries/GetItem/GetItemQuery.cs`
- Create: `src/StuffTracker.Application/Items/Queries/GetItems/GetItemsQuery.cs`
- Create: `src/StuffTracker.Application/Items/Dtos/ItemDto.cs`
- Create: `src/StuffTracker.Infrastructure/Configuration/ItemConfiguration.cs`
- Create: `src/StuffTracker.Infrastructure/Repositories/ItemsRepository.cs`
- Create: `src/StuffTracker.API/Controllers/ItemsController.cs`

### 5.1 — Item-entitet + migration

- [ ] Skapa `Item`: Id, Name, Description, HomeId (FK), LocationId (FK), CategoryId (FK, nullable), Status (enum: InPlace, Lent, Lost, Unsorted), IsDeleted, CreatedAt, UpdatedAt
- [ ] Skapa `ItemStatus` enum
- [ ] EF-konfiguration med FK-constraints
- [ ] Migration
- [ ] Commit: `feat: add Item entity and migration`

### 5.2 — CreateItem + GetItem

- [ ] `CreateItemCommand`: Name, Description, HomeId, LocationId (nullable), CategoryId (nullable)
- [ ] Handler-logik:
  - Verifiera att användaren äger hemmet
  - Om LocationId anges: verifiera att location tillhör hemmet
  - Om LocationId **inte** anges: sätt till Unsorted-noden, Status = Unsorted
  - Om LocationId anges: Status = InPlace
- [ ] `GetItemQuery` + handler: hämta item med Id, verifiera behörighet
- [ ] `ItemDto`: Id, Name, Description, LocationId, LocationName, CategoryId, CategoryName, Status, CreatedAt
- [ ] `ItemsController` med `POST /api/homes/{homeId}/items` och `GET /api/items/{id}`
- [ ] Testa: skapa item med plats, utan plats, hämta
- [ ] Commit: `feat: add CreateItem and GetItem`

### 5.3 — GetItems med filtrering + paginering

- [ ] `GetItemsQuery`: HomeId, LocationId (optional filter), CategoryId (optional filter), Status (optional filter), Page (default 1), PageSize (default 20)
- [ ] Handler: bygg query med valfria filter, offset-paginering
- [ ] Returnera `PagedResult<ItemDto>` med TotalCount, Page, PageSize, Items
- [ ] `GET /api/homes/{homeId}/items?locationId=...&categoryId=...&status=...&page=1&pageSize=20`
- [ ] Testa: skapa flera items, filtrera på location, filtrera på kategori, paginera
- [ ] Commit: `feat: add GetItems with filtering and pagination`

### 5.4 — MoveItem + UpdateItem + DeleteItem

- [ ] `MoveItemCommand`: ItemId, NewLocationId — validera att location tillhör samma hem
- [ ] `UpdateItemCommand`: ItemId, Name, Description, CategoryId, Status
- [ ] `DeleteItemCommand`: ItemId — soft delete
- [ ] Controller-endpoints: `PATCH /api/items/{id}/move`, `PATCH /api/items/{id}`, `DELETE /api/items/{id}`
- [ ] Testa hela flödet: skapa item → flytta → uppdatera → ta bort
- [ ] Commit: `feat: add MoveItem, UpdateItem, DeleteItem`

### 5.5 — Koppla DeleteLocation till Items

- [ ] Gå tillbaka till `DeleteLocationCommandHandler`
- [ ] När en plats soft-deletas: flytta alla items i den platsen (och child-platser) till Unsorted-noden, sätt Status = Unsorted
- [ ] Testa: skapa plats med items, ta bort platsen, verifiera att items hamnar i Unsorted
- [ ] Commit: `feat: move items to Unsorted when location is deleted`

---

## Skiva 6 — Sök med hierarki-svar

**Mål:** Sök på saknamn, få tillbaka full platshierarki.

**Files:**
- Create: `src/StuffTracker.Application/Items/Queries/SearchItems/SearchItemsQuery.cs`
- Create: `src/StuffTracker.Application/Items/Queries/SearchItems/SearchItemsQueryHandler.cs`
- Create: `src/StuffTracker.Application/Items/Dtos/SearchResultDto.cs`
- Modify: `src/StuffTracker.Domain/Repositories/IItemsRepository.cs`
- Modify: `src/StuffTracker.Infrastructure/Repositories/ItemsRepository.cs`
- Modify: `src/StuffTracker.API/Controllers/ItemsController.cs`

### 6.1 — Research rekursiv CTE

- [ ] Skriv och testa en rekursiv CTE direkt mot databasen (SQL Server Management Studio eller Azure Data Studio)
- [ ] CTE:n ska: givet en LocationId, returnera hela kedjan uppåt till Home
- [ ] Exempelfråga:
  ```sql
  WITH LocationHierarchy AS (
      SELECT Id, Name, ParentId, 0 AS Level
      FROM Locations WHERE Id = @LocationId
      UNION ALL
      SELECT l.Id, l.Name, l.ParentId, lh.Level + 1
      FROM Locations l
      INNER JOIN LocationHierarchy lh ON l.Id = lh.ParentId
      WHERE l.IsDeleted = 0
  )
  SELECT * FROM LocationHierarchy ORDER BY Level DESC;
  ```
- [ ] Verifiera att den returnerar korrekt kedja: Home > Room > Storage > Storage > ...
- [ ] Commit: `docs: document recursive CTE for location hierarchy`

### 6.2 — SearchItems endpoint

- [ ] `SearchItemsQuery`: HomeId, SearchTerm
- [ ] Repository-metod som:
  1. Söker items via `LIKE '%term%'` (case-insensitive) på Name
  2. För varje matchande item, kör rekursiv CTE för att hämta platshierarkin
  3. Returnerar `SearchResultDto` med ItemName, ItemDescription, LocationPath (t.ex. "Stugan > Förrådet > Verktygslådan > Övre facket")
- [ ] Implementera med `FromSqlRaw` eller raw SQL via DbContext
- [ ] `GET /api/homes/{homeId}/items/search?q=skruvdragare`
- [ ] Testa: skapa items i djup hierarki, sök → verifiera att platshierarkin visas korrekt
- [ ] Commit: `feat: add item search with full location hierarchy`

---

## Skiva 7 — Felhantering, logging och polish

**Mål:** Produktionskvalitet: konsekvent felhantering, structured logging, snygga API-svar.

**Files:**
- Create: `src/StuffTracker.API/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `src/StuffTracker.Application/Common/Behaviors/LoggingBehavior.cs`
- Modify: `src/StuffTracker.API/Program.cs`

### 7.1 — Global exception-handling middleware

- [ ] Skapa `ExceptionHandlingMiddleware` som fångar exceptions och mappar till HTTP-svar:
  - `NotFoundException` → 404
  - `BusinessRuleException` → 422
  - `FluentValidation.ValidationException` → 400
  - `UnauthorizedAccessException` → 403
  - Allt annat → 500 (logga, returnera generisk feltext)
- [ ] Konsekvent JSON-format: `{ "status": 404, "error": "Not Found", "message": "Location with id X was not found" }`
- [ ] Registrera i `Program.cs`
- [ ] Testa: trigga olika felfall, verifiera JSON-svar
- [ ] Commit: `feat: add global exception handling middleware`

### 7.2 — MediatR LoggingBehavior

ValidationBehavior skapades redan i skiva 0.6. Nu lägger vi till logging.

- [ ] Skapa `LoggingBehavior<TRequest, TResponse>` som loggar command/query-namn, användare, och duration
- [ ] Registrera som `IPipelineBehavior` i DI
- [ ] Commit: `feat: add MediatR logging behavior`

### 7.3 — Serilog

- [ ] Installera `Serilog.AspNetCore`
- [ ] Konfigurera i `Program.cs` med `UseSerilog()`
- [ ] Console-sink med structured logging
- [ ] Valfritt: fil-sink under utveckling
- [ ] Testa: gör API-anrop, se structured logs i terminalen
- [ ] Commit: `feat: add Serilog structured logging`

---

## Skiva 8 — Azure deployment + CI/CD

**Mål:** API:t körs i Azure med automatisk deployment från GitHub.

### 8.1 — Azure-resurser

- [ ] Skapa Azure App Service (Basic plan)
- [ ] Skapa Azure SQL Database
- [ ] Konfigurera connection string i App Service Configuration
- [ ] JWT-hemligheter i App Service Configuration

### 8.2 — Environment-konfiguration

- [ ] Skapa `appsettings.Production.json` utan känsliga värden
- [ ] Health check endpoint: `GET /health`
- [ ] Verifiera att EF Core migrations körs vid startup (eller som del av deployment)
- [ ] Commit: `feat: add production configuration and health check`

### 8.3 — GitHub Actions CI/CD

- [ ] Skapa `.github/workflows/deploy.yml`:
  - Trigger: push to main
  - Build → Test → Deploy to Azure App Service
- [ ] Testa: pusha, se deployment köras, verifiera att API:t svarar på Azure-URL
- [ ] Commit: `feat: add GitHub Actions deployment pipeline`

---

## Skiva 9 — AI-genererad frontend

**Mål:** Enkel frontend som demo.

### 9.1 — Exportera OpenAPI spec

- [ ] Hämta `/openapi/v1.json` från ditt körande API
- [ ] Spara som referens

### 9.2 — Generera frontend

- [ ] Använd OpenAPI-spec som input till AI-verktyg
- [ ] Bygg React + Tailwind-app med: login, lista hem, navigera platshierarki, söka items
- [ ] Deploya som Azure Static Web App eller liknande

---

## Sammanfattning

| Skiva | Fokus | Nyckelbegrepp |
|-------|-------|---------------|
| 0 | Stabilisera grund | Swagger, routes, HomeId-denorm, Unsorted-nod, exceptions, ValidationBehavior |
| 1 | Skapa platser | Hierarkivalidering, parent-child-regler (Room→Home, Storage→Room/Storage) |
| 2 | Location CRUD | GetByHome, Update, Soft delete, rekursiv child-delete |
| 3 | Auth | Identity, JWT, UserHome, behörighetskontroll |
| 4 | Categories | Seed data, global vs hemspecifik |
| 5 | Items CRUD | FK-validering, paginering, filtrering, MoveItem |
| 6 | Sök med hierarki | Rekursiv CTE, raw SQL i EF Core |
| 7 | Polish | Middleware, pipeline behaviors, Serilog |
| 8 | Deployment | Azure, CI/CD, environment config |
| 9 | Frontend | OpenAPI, AI-genererad |

---

## Designbeslut och tips

### LocationType — Position borttaget (2026-03-25)
`Position` togs bort ur `LocationType`-enumet. Det adderade ett redundant typ-värde som överlappade helt med nested `Storage`. En låda i en garderob är en `Storage` under en `Storage` — inte en `Position`. Att tvinga ett annat typnamn på sista nivån skapade förvirring utan att tillföra något. Hierarkin är nu: `Home → Room → Storage → Storage → ...` med godtycklig nesting-djup.

### HomeId på Location (denormalisering)
HomeId läggs till i skiva 0.2 som en medveten denormalisering. Det är tekniskt redundant (kan härledas via parent-kedjan) men gör queries **mycket** enklare. Alla frågor av typen "hämta alla platser i hemmet" blir en enkel WHERE istället för rekursiv traversering. HomeId sätts en gång vid skapande och ändras aldrig (locations kan inte flytta mellan hem).

### Var placeras hierarkivalidering?
Hierarkiregeln "Room måste ha Home som parent" kräver ett DB-uppslag för att kontrollera parent-locationens typ. FluentValidation-validatorn hanterar input-validering (rätt typ, namn finns osv). Hierarkivalidering placeras i handlern eftersom den behöver repository-access. Kastas som `BusinessRuleException` → mappar till HTTP 422.

### Guid-generering
Generera Guid i handler eller command (inte i repository/EF). Det ger dig kontroll och gör det möjligt att returnera Id direkt utan extra roundtrip.

### Transaktioner
Där du gör flera databasändringar i samma handler (t.ex. CreateHome + Unsorted-nod): se till att du anropar `SaveChangesAsync()` en enda gång efter alla ändringar. EF Core:s change tracker hanterar detta — alla `Add()`-anrop sparas som en batch vid `SaveChangesAsync()`.

### REST-routes
Konsekvent mönster genom hela API:t:
- `POST/GET /api/homes` — hemoperationer
- `POST/GET /api/homes/{homeId}/locations` — platser inom ett hem
- `POST/GET /api/homes/{homeId}/items` — saker inom ett hem
- `POST/GET /api/homes/{homeId}/categories` — kategorier inom ett hem
- `PUT/DELETE /api/locations/{id}` — operationer på enskild location
- `PUT/DELETE /api/items/{id}` — operationer på enskild item

### Testning
Planen fokuserar på manuell testning via Scalar. Vill du lägga till automatiserade tester (unit tests för handlers, integration tests för API) är det ett naturligt tillägg efter att grundfunktionaliteten fungerar.
