# StuffTracker — Implementation Plan

Varje skiva är en vertikal slice: den går hela vägen genom Domain → Application → Infrastructure → API.
Efter varje skiva har du något körbart att testa i Swagger/Scalar.

**Tumregel:** Gör klart en skiva innan du påbörjar nästa. Det är frestande att hoppa framåt — motstå det.

---

## Skiva 1 — Projektstruktur + "Skapa ett hem"

**Mål:** Hela Clean Architecture-skelettet på plats, en databas som körs, och ett enda endpoint som bevisar att allt hänger ihop.

### Vad du bygger

- Solution med fyra projekt: `Domain`, `Application`, `Infrastructure`, `API`
- Projektreferenser enligt Clean Architecture (API → Application, Application → Domain, Infrastructure → Domain, API → Infrastructure för DI-registrering)
- `Location`-entiteten i Domain (med `LocationType`-enum)
- `DbContext` i Infrastructure med EF Core + SQL Server
- Första migrationen
- `CreateHome`-command (MediatR) + validator (FluentValidation)
- `GetHome`-query
- `LocationsController` med `POST /api/locations/homes` och `GET /api/locations/homes/{id}`
- **Ingen auth ännu** — hårdkoda ett fake userId för nu

### Varför just detta?

Det här är den största skivan trots att featuren är minimal. Du sätter upp hela mappstrukturen, NuGet-paketen, DI-registreringen, EF Core-konfigurationen, MediatR-pipeline, och Swagger. Allt detta återanvänds i varje framtida skiva.

### Klart när

- Du kan köra API:t lokalt
- `POST /api/locations/homes` skapar ett hem i databasen
- `GET /api/locations/homes/{id}` returnerar det
- En `Unsorted`-nod skapas automatiskt när hemmet skapas
- Du ser endpoints i Swagger/Scalar

---

## Skiva 2 — Platshierarkin (CRUD)

**Mål:** Komplett CRUD för hela Location-hierarkin, inklusive valideringsregler.

### Vad du bygger

- `CreateLocation`-command för Room, Storage, Position (inte bara Home som i skiva 1)
- Valideringslogik: kontrollera att `LocationType` + `ParentId` följer reglerna (Room under Home, Storage under Room/Storage, Position under Storage)
- `GetLocationsByHome`-query — hämta alla platser för ett hem (flat lista eller trädstruktur)
- `UpdateLocation`-command (byt namn/beskrivning)
- `DeleteLocation`-command med soft delete + flytta items till Unsorted-noden
- Globalt query-filter i EF Core: `entity.HasQueryFilter(l => !l.IsDeleted)` så att soft-deletade rader automatiskt exkluderas

### Varför just detta?

Location-hierarkin är fundamentet som Items bygger på. Du vill att den är solid innan du lägger till saker ovanpå. Valideringslogiken (vilken typ av nod får ligga under vilken) är viktig att få rätt nu.

### Klart när

- Du kan skapa en hel hierarki: Hem → Rum → Förvaringsplats → Position
- Storage under Storage fungerar (nästlad förvaring)
- Felaktiga kombinationer avvisas (t.ex. Room under Storage ger 400 Bad Request)
- Delete soft-deletar och Unsorted-noden skyddas (kan ej tas bort)

---

## Skiva 3 — Auth (Identity + JWT)

**Mål:** Riktiga användare som kan registrera sig, logga in, och bara se sina egna hem.

### Vad du bygger

- ASP.NET Core Identity konfigurerat med din `User`-entitet
- Registrerings-endpoint (`POST /api/auth/register`)
- Login-endpoint (`POST /api/auth/login`) som returnerar JWT-token
- JWT Bearer auth konfigurerat i `Program.cs`
- `[Authorize]`-attribut på controllers
- `UserHome`-entiteten + migration
- Koppling: när ett hem skapas, skapas en `UserHome`-rad med `Role = Owner`
- Behörighetskontroll: alla Location-queries filtrerar på användarens hem via `UserHome`
- Ta bort det hårdkodade fake userId:t från skiva 1

### Varför just detta?

Auth måste in innan Items, annars bygger du allt utan behörighetskontroll och får retrofita det efteråt (smärtsamt). Det är också en bra övning — Identity + JWT har många rörliga delar.

### Klart när

- Registrering + login fungerar, du får tillbaka en JWT-token
- Utan token → 401 Unauthorized
- Med token → du ser bara dina egna hem
- Två olika användare ser inte varandras data

---

## Skiva 4 — Categories (CRUD)

**Mål:** Kategorier på plats — både globala och hemspecifika.

### Vad du bygger

- `Category`-entiteten i Domain
- Migration
- Seed globala kategorier (t.ex. "Tools", "Electronics", "Documents", "Kitchen", "Clothing")
- `CreateCategory`-command (skapar hemspecifik kategori)
- `GetCategories`-query (returnerar globala + hemspecifika)
- `UpdateCategory` och `DeleteCategory` (soft delete, bara hemspecifika — globala kan inte ändras)
- `CategoriesController`

### Varför just detta?

Categories är en enklare entitet som Items har en FK till. Genom att bygga den nu slipper du skapa Items utan kategori-stöd och sedan lägga till det.

### Klart när

- Globala kategorier finns efter seed/migration
- Du kan skapa hemspecifika kategorier
- `GET /api/homes/{homeId}/categories` returnerar globala + hemspecifika
- Globala kategorier kan inte ändras/raderas via API

---

## Skiva 5 — Items (CRUD)

**Mål:** Saker kan skapas, kopplas till platser, och hanteras.

### Vad du bygger

- `Item`-entiteten i Domain
- Migration
- `CreateItem`-command med validering (Name krävs, LocationId måste tillhöra samma hem som HomeId)
- Logik: om ingen LocationId anges → sätt den automatiskt till Unsorted-noden + Status = Unsorted
- `GetItem`-query
- `GetItems`-query med filtrering (by location, by category, by status) + offset-paginering
- `UpdateItem`-command (ändra namn, beskrivning, kategori, status)
- `MoveItem`-command (flytta till ny plats — separat command eftersom det uppdaterar LocationId + LastModifiedByUserId)
- `DeleteItem`-command (soft delete)
- `ItemsController`

### Varför just detta?

Nu börjar appen bli användbar. Du har platser, kategorier, och saker. Det enda som saknas från grundfunktionaliteten är sökning.

### Klart när

- Skapa en sak kopplad till en plats fungerar
- Skapa utan plats → hamnar i Unsorted
- Flytta en sak mellan platser fungerar
- Filtrering på plats, kategori, status fungerar
- Paginering fungerar (page + pageSize som query params)

---

## Skiva 6 — Sök med hierarki-svar

**Mål:** Sök på saknamn och få tillbaka den fullständiga platskedjan.

### Vad du bygger

- Research + implementera rekursiv CTE i SQL Server (detta är den tekniskt svåraste delen i hela steg 1)
- `SearchItems`-query som tar en sökterm och returnerar matchande items med full platshierarki
- Svarsformat: `"Skruvdragaren finns i: Stugan > Förrådet > Verktygslådan > Övre facket"`
- Anropa CTE:n via `FromSqlRaw` eller stored procedure
- `GET /api/homes/{homeId}/items/search?q=skruvdragare`

### Varför en egen skiva?

Rekursiv CTE är en teknik du förmodligen inte jobbat med förut. Det förtjänar sitt eget fokus. Du kommer behöva testa och debugga SQL-frågan separat innan du integrerar den med EF Core.

### Klart när

- Sökning på del av namn hittar rätt sak
- Svaret inkluderar hela platshierarkin uppåt till hemmet
- Case-insensitive sökning fungerar

---

## Skiva 7 — Felhantering, logging och polish

**Mål:** Produktionskvalitet på felhantering och observability.

### Vad du bygger

- Global exception-handling middleware (fånga upp unhandled exceptions, returnera korrekt HTTP-status)
- Anpassade domain exceptions (t.ex. `NotFoundException`, `ForbiddenException`)
- Serilog-konfiguration (structured logging till console + ev. fil)
- Request/response-logging via MediatR pipeline behavior
- Konsekvent API-responsformat (alla fel returnerar samma JSON-struktur)
- Swagger/OpenAPI-konfiguration med auth-stöd (så du kan testa med JWT i Swagger UI)

### Klart när

- Alla felfall returnerar konsekvent JSON (inte stack traces)
- Loggar visar request/response-info
- Swagger UI har en "Authorize"-knapp där du kan klistra in JWT

---

## Skiva 8 — Azure deployment + CI/CD

**Mål:** API:t körs i molnet med automatisk deployment.

### Vad du bygger

- Azure App Service (skapa via Azure Portal eller CLI)
- Azure SQL database
- Connection string i Azure App Service Configuration (inte i kod)
- GitHub Actions workflow: build → test → deploy on push to main
- Environment-specifik konfiguration (Development vs Production)
- Health check endpoint (`GET /health`)

### Klart när

- API:t är nåbart på en publik URL
- Push till main → automatisk deployment
- Databasen körs i Azure SQL
- Connection strings/hemligheter ligger inte i källkoden

---

## Skiva 9 — AI-genererad frontend

**Mål:** En enkel frontend som demo.

### Vad du bygger

- Exportera OpenAPI-specifikationen från ditt körande API
- Använd spec:en som underlag för att AI-generera en React + Tailwind-frontend
- Grundläggande vyer: login, lista hem, visa platshierarki, lista/sök saker
- Deploy som static web app (Azure Static Web Apps eller liknande)

### Klart när

- Du kan logga in, se dina hem, navigera platshierarkin, och söka efter saker via ett webbgränssnitt

---

## Sammanfattning

| Skiva | Fokus                             | Nyckelbegrepp du lär dig                                          |
| ----- | --------------------------------- | ----------------------------------------------------------------- |
| 1     | Projektstruktur + första endpoint | Clean Architecture, EF Core setup, MediatR, Swagger               |
| 2     | Location CRUD                     | Self-referencing FK, valideringslogik, soft delete, query filters |
| 3     | Auth                              | Identity, JWT, behörighetskontroll, claims                        |
| 4     | Categories                        | Seed data, global vs scoped data                                  |
| 5     | Items CRUD                        | FK-validering, paginering, filtrering                             |
| 6     | Sökning med hierarki              | Rekursiv CTE, raw SQL i EF Core                                   |
| 7     | Polish                            | Middleware, structured logging, error handling                    |
| 8     | Deployment                        | Azure, CI/CD, environment config                                  |
| 9     | Frontend                          | OpenAPI, AI-genererad kod                                         |

> **Steg 2 (AI-integration)** — skivorna för bildhantering och semantisk sökning bygger du efter att allt ovan är klart och deployat.
