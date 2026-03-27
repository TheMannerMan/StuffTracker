# StuffTracker — Data Model

## Entities Overview

- [User](#user)
- [UserHome](#userhome)
- [Location](#location)
- [Category](#category)
- [Item](#item)

---

## User

Extends ASP.NET Core `IdentityUser`. Only application-specific fields are listed below.

| Field | Type | Nullable | Notes |
| --- | --- | --- | --- |
| `ProfileImageUrl` | string | Yes | Populated in Step 2 when image upload is implemented |
| `PreferredLanguage` | string | No  | Default: `"en"`. App is English-first |

---

## UserHome

A home can be shared between multiple users — for example, family members living together. This junction table connects users to homes and defines their role within that home. All members of a home have access to the same locations, items, and custom categories within it.

| Field | Type | Nullable | Notes |
| --- | --- | --- | --- |
| `UserId` | GUID | No  | FK → User |
| `LocationId` | GUID | No  | FK → Location (must be `LocationType.Home`) |
| `Role` | enum | No  | `Owner`, `Member` |

> **Invite flow is not part of Step 1.** To share a home, users will eventually be able to invite others by email. The `UserHome` table is already structured to support this, but the invitation feature itself will be built in a later iteration.

---

## Location

Represents any physical place in a hierarchy. Uses a self-referencing structure where each node can have a parent.

| Field | Type | Nullable | Notes |
| --- | --- | --- | --- |
| `Id` | GUID | No  |     |
| `Name` | string | No  |     |
| `Description` | string | Yes | Optional free-text comment, e.g. "Cabinet above the stove, left side" |
| `ImageUrl` | string | Yes | Populated in Step 2 when image upload is implemented |
| `LocationType` | enum | No  | See values below |
| `ParentId` | GUID | Yes | FK → Location. `null` only for `Home` nodes. |
| `IsDeleted` | bool | No  | Soft delete flag |
| `DeletedAt` | DateTime | Yes | Set when soft deleted |
| `CreatedAt` | DateTime | No  |     |
| `UpdatedAt` | DateTime | No  |     |

### LocationType Enum

| Value | Description | Allowed Parent |
| --- | --- | --- |
| `Home` | Top-level dwelling, e.g. "Apartment", "Cottage" | None (`ParentId` is null) |
| `Room` | A room inside a home, e.g. "Living Room" | `Home` |
| `Storage` | A storage unit, e.g. cabinet, drawer, shelf, toolbox | `Room` or `Storage` |
| `Unsorted` | System-generated catch-all node per home | `Home` |

> **Unsorted node:** Created automatically when a home is created. Scoped to the home — shared by all members. Cannot be edited or deleted by the user. Items land here when: (1) added without a location, or (2) their assigned location is deleted.
>
> **Storage nesting:** `Storage → Storage` is intentionally allowed — and is the mechanism for expressing fine-grained locations. A wardrobe can contain drawers, a toolbox can contain compartments, etc. There is no separate "Position" type; all levels below Room are modelled as Storage.
>
> **Design decision (2026-03-25):** `Position` was removed from `LocationType`. It added a redundant type that overlapped completely with nested `Storage`. Users were likely to be confused about whether a "drawer" is a Storage or a Position. Removing it simplifies validation rules and the mental model.

---

## Category

Stores both predefined (global) and user-created categories for items.

| Field | Type | Nullable | Notes |
| --- | --- | --- | --- |
| `Id` | GUID | No  |     |
| `Name` | string | No  |     |
| `Icon` | string | Yes | Icon name from a frontend icon library (e.g. `"wrench"`, `"box"`) |
| `Color` | string | Yes | Hex color code, e.g. `"#FF5733"` |
| `HomeId` | GUID | Yes | `null` = global/predefined. Set = user-created, scoped to that home |
| `IsDeleted` | bool | No  | Soft delete flag |
| `DeletedAt` | DateTime | Yes | Set when soft deleted |
| `CreatedAt` | DateTime | No  |     |
| `UpdatedAt` | DateTime | No  |     |

> **Global categories** are seeded at deployment and available to all users.
> 
> **User-created categories** are scoped to a home (not a single user), so all members of the same home share the same custom categories.
> 
> **AI suggestion (Step 2):** When a user uploads an image of an item, the AI will suggest a single category. The user confirms or overrides the suggestion.

---

## Item

Represents a physical object owned by a user, stored somewhere in the location hierarchy.

| Field | Type | Nullable | Notes |
| --- | --- | --- | --- |
| `Id` | GUID | No  |     |
| `Name` | string | No  |     |
| `Description` | string | Yes | Optional free-text description |
| `ImageUrl` | string | Yes | Populated in Step 2 when image upload is implemented |
| `Status` | enum | No  | See values below |
| `LocationId` | GUID | No  | FK → Location. Always set — never null. Points to `Unsorted` node if no location is chosen |
| `HomeId` | GUID | No  | FK → Location (must be `LocationType.Home`). Items are always scoped to one home |
| `CategoryId` | GUID | Yes | FK → Category. Optional |
| `CreatedByUserId` | GUID | No  | FK → User |
| `LastModifiedByUserId` | GUID | No  | FK → User. Updated on every change, including location moves |
| `IsDeleted` | bool | No  | Soft delete flag |
| `DeletedAt` | DateTime | Yes | Set when soft deleted |
| `CreatedAt` | DateTime | No  |     |
| `UpdatedAt` | DateTime | No  |     |

### ItemStatus Enum

| Value | Description |
| --- | --- |
| `Unsorted` | Item has no assigned location (sits in the Unsorted node) |
| `Placed` | Item is at its registered location |
| `InUse` | Item is currently being used and is temporarily away from its registered location |
| `Lent` | Item has been lent to someone |
| `Lost` | Item has been marked as lost |

> **An item always belongs to exactly one home.** If the same physical object exists in two homes, it is registered as two separate items with no relation between them.
> 
> **An item always has a LocationId.** When status is `Unsorted`, `LocationId` points to the system-generated `Unsorted` node for the user.

---

## Relationships Summary

```
User ──────────────── UserHome ──────────────── Location (Home)
                      (Owner/Member)                    │
                                                        │ ParentId
                                              ┌─────────┴──────────┐
                                           Location             Location
                                           (Room)               (Unsorted)
                                              │
                                           Location
                                           (Storage)
                                              │
                                           Location
                                           (Storage)
                                              │
                                            ...

Item ──── LocationId ──── Location (any type, including Unsorted)
Item ──── HomeId ──────── Location (Home only)
Item ──── CategoryId ──── Category (optional)
Item ──── CreatedByUserId / LastModifiedByUserId ──── User

Category ──── HomeId ──── Location (Home, null if global)
```

---

## General Notes

- **Soft delete** is applied to `Location`, `Category`, and `Item`. Hard deletes are not used.
- **Pagination** uses offset-based paging for all list endpoints.
- **Images** (`ProfileImageUrl`, `Location.ImageUrl`, `Item.ImageUrl`) are stored as URL strings. Actual upload functionality is implemented in Step 2 using Azure Blob Storage.
- **Invite flow** for sharing homes is deferred to a later iteration. The `UserHome` table is in place to support it.