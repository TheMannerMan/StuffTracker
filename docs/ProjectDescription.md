# StuffTracker — Håll koll på dina prylar

## Bakgrund och syfte

Portfolio-projekt för att demonstrera backend-utveckling med .NET/C# och Clean Architecture.
Projektet byggs i två steg med ökande teknisk komplexitet och syftar till att visa:

- Datamodellering med hierarkiska relationer i EF Core
- Clean Architecture med CQRS (MediatR)
- Autentisering och auktorisering
- Azure-deployment med CI/CD
- AI-integration (steg 2)

Referensimplementation för arkitekturmönster: [Restaurants_WebApi](https://github.com/TheMannerMan/Restaurants_WebApi)

---

## Konceptbeskrivning

Användaren loggar in och skapar en digital modell av sitt hem med platshierarki.
Sedan registreras ägodelar och kopplas till specifika platser.
Appen svarar på frågan: **"Var ligger min [sak]?"**

### Platshierarki

```
Hem → Rum → Förvaringsplats → Förvaringsplats → ...
```

Förvaringsplatser kan nästlas godtyckligt djupt för att modellera verklighetens struktur.

Exempel:

```
Lägenheten → Vardagsrummet → TV-bänken → Överlådan
Lägenheten → Köket → Skåpet ovanför spisen → Vänster hylla
Stugan → Förrådet → Verktygslådan → Övre facket
```

En användare kan ha flera hem (t.ex. lägenhet + stuga).

### Saker (Items)

Varje sak har:

- Namn
- Beskrivning (valfri)
- Kategori (valfri, t.ex. "Verktyg", "Elektronik", "Dokument")
- Plats (koppling till en nod i platshierarkin)
- Bild (steg 2)

---

## Steg 1 — Backend-grund

### Funktionella krav

- **Autentisering**: Registrering och inloggning med ASP.NET Core Identity + JWT
- **Platser (CRUD)**: Skapa, hämta, uppdatera, ta bort platser i hierarkin
- **Saker (CRUD)**: Registrera saker, koppla till plats, flytta mellan platser
- **Sök**: Sök på sakens namn → returnera plats med full hierarki ("Skruvdragaren finns i: Stugan > Förrådet > Verktygslådan > Övre facket")
- **Filtrering**: Visa alla saker i ett rum, alla saker i en kategori
- **Behörighet**: Användare ser bara sina egna hem och saker

### Tekniska beslut att fatta

| Beslut | Frågor att utforska |
| --- | --- |
| **Hierarkin** | Self-referencing entity med ParentId? Separata tabeller per nivå? Hur djup hierarki tillåts? |
| **Radering** | Vad händer med saker när en plats tas bort? Cascade delete? Soft delete? Flytta till "Osorterat"? |
| **Statusflöde** | Behöver saker status? (t.ex. "utlånad", "på sin plats", "försvunnen") |
| **Sök** | EF Core LINQ-baserad sökning räcker i steg 1. Full-text search kan övervägas. |
| **Paginering** | Behövs för listor med många saker. Offset-baserad eller cursor-baserad? |

### Teknisk stack (steg 1)

- ASP.NET Core Web API (.NET 10)
- Clean Architecture (Domain, Application, Infrastructure, API)
- CQRS med MediatR
- EF Core med SQL Server / PostgreSQL
- FluentValidation
- ASP.NET Core Identity + JWT Bearer auth
- Serilog för logging
- Azure App Service (deployment)
- Azure SQL (databas)
- GitHub Actions (CI/CD)

### Arkitekturmönster (samma som Restaurants_WebApi)

```
┌─────────────────────────────────┐
│  API (Presentation)             │
│  Controllers, Middleware        │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│  Application (Use Cases)        │
│  Commands, Queries, DTOs,       │
│  Validators, Mapping            │
└────────────┬────────────────────┘
             ↓
┌─────────────────────────────────┐
│  Domain (Business Logic)        │
│  Entities, Interfaces,          │
│  Exceptions                     │
└────────────┬────────────────────┘
             ↑
┌─────────────────────────────────┐
│  Infrastructure (External)      │
│  EF Core, Repositories,        │
│  Identity, Auth Handlers        │
└─────────────────────────────────┘
```

---

## Steg 2 — AI-integration och bildhantering

### Nya funktionella krav

- **Bilduppladdning**: Varje sak kan ha en bild
- **Semantisk sökning**: Sök med beskrivning istället för exakt namn
  - Exempel: "det röda verktyget för att dra åt skruvar" → hittar "Skruvdragare"
  - Exempel: "dokumenten från banken" → hittar "Bolånepapper"

### Tekniska tillägg (steg 2)

- **Azure Blob Storage** för bildlagring
- **Embeddings-API** (OpenAI eller Anthropic) för att generera vektorrepresentationer av sakens namn + beskrivning + bildanalys
- **Vektorlagring** för semantisk sökning (PostgreSQL med pgvector, eller Azure AI Search)
- **Bildbeskrivning via AI**: Skicka bilden till ett vision-API, få tillbaka en textbeskrivning som lagras och indexeras

### Flöde vid semantisk sökning

```
Användaren söker: "grejerna för att laga cykeln"
    ↓
Sökfrågan omvandlas till embedding-vektor
    ↓
Jämför mot lagrade vektorer för alla användarens saker
    ↓
Returnerar närmaste matchningar:
  1. Däckjärn (Stugan > Förrådet > Cykelhyllan)
  2. Pumpslang (Stugan > Förrådet > Verktygslådan)
  3. Lagerlåsenyckel (Lägenheten > Hallen > Nyckelskåpet)
```

---

## Frontend-strategi

Frontend är **inte** fokus — den existerar som testverktyg och demo.

**Tillvägagångssätt:** Exportera OpenAPI-specifikationen (JSON) från API:t och använd den som underlag för att AI-generera en enkel frontend (React + Tailwind eller liknande).

API:ts OpenAPI-spec finns typiskt på `/openapi/v1.json` när applikationen körs.

---

## Iterationsplan

### Iteration 1: Grund

- Projektstruktur med Clean Architecture
- User-entitet med Identity + JWT
- Plats-hierarki (Hem → Rum → Förvaringsplats → Position)
- CRUD för platser

### Iteration 2: Saker

- Item-entitet med koppling till plats
- CRUD för saker
- Sök på namn med fullständig platshierarki i svaret
- Filtrering och paginering

### Iteration 3: Deployment och polish

- Azure App Service deployment
- CI/CD med GitHub Actions
- Azure SQL databas
- Grundläggande felhantering och logging
- AI-genererad frontend

### Iteration 4: Bildhantering (steg 2)

- Azure Blob Storage-integration
- Bilduppladdning kopplad till items
- Bildbeskrivning via AI vision-API

### Iteration 5: Semantisk sökning (steg 2)

- Embedding-generering för items (namn + beskrivning + bildbeskrivning)
- Vektorlagring
- Semantisk sökendpoint

---

## Utvecklingsmetod

- **Spec-driven workflow** med Claude Code
- **Git worktrees** för parallellt arbete på features
- **GitHub Issues** för att tracka arbete
- **PR-baserat workflow** med feature branches

---

## Kontextlänkar

- Restaurants_WebApi (referensimplementation): https://github.com/TheMannerMan/Restaurants_WebApi
- Denna fil skapad: 2026-03-18
- Senast uppdaterad: 2026-03-25 (Position borttaget ur LocationType — täcks av Storage-nesting)