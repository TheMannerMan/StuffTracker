# Endpoints — Steg 1

## Hem

| Metod  | Endpoint | Syfte            |
|--------|----------|------------------|
| GET    | `/homes` | Lista användarens hem |
| POST   | `/homes` | Skapa hem        |

## Platser

| Metod  | Endpoint                       | Syfte              |
|--------|--------------------------------|--------------------|
| GET    | `/homes/{homeId}/locations`    | Hämta platsträdet  |
| POST   | `/homes/{homeId}/locations`    | Skapa ny plats     |
| PUT    | `/locations/{id}`              | Uppdatera plats    |
| DELETE | `/locations/{id}`              | Ta bort plats      |

## Saker

| Metod  | Endpoint              | Syfte                  |
|--------|-----------------------|------------------------|
| GET    | `/locations/{id}/items` | Saker på en plats    |
| GET    | `/items`              | Sök/filtrera saker     |
| POST   | `/items`              | Skapa sak              |
| PUT    | `/items/{id}`         | Uppdatera/flytta sak   |
| DELETE | `/items/{id}`         | Ta bort sak            |