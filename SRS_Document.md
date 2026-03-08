# Software Requirements Specification (SRS)
## Employee & Family Registry — Bangladesh Context

**Version:** 1.0  
**Date:** March 8, 2026  
**Project:** Full-Stack Employee Management System

---

## 1. System Scope

### 1.1 What the System Does

- **Employee Profile Management**: Full CRUD (Create, Read, Update, Delete) operations for employee records including Name, NID, Phone, Department, and Basic Salary.
- **Family Data Management**: Maintains one-to-one Spouse relationships (Name, NID) and one-to-many Children relationships (Name, Date of Birth) per employee.
- **Global Search**: A single search input that filters employees by Name, NID, or Department using case-insensitive matching.
- **PDF Reporting**:
  - **Table View PDF**: Exports the current filtered employee list as a formatted PDF table.
  - **Individual CV PDF**: Generates a detailed PDF summary of a single employee including their family details.
- **Role-Based Access Control**: Two roles — **Admin** (full CRUD access) and **Viewer** (read-only access) — secured via JWT authentication.

### 1.2 What the System Does NOT Do

- Does **not** handle payroll calculations, tax deductions, or salary disbursements.
- Does **not** support file/document uploads (photos, certificates, etc.).
- Does **not** provide audit trails or change history for records.
- Does **not** support multi-tenancy or organizational hierarchies.
- Does **not** handle employee attendance, leave management, or time tracking.
- Does **not** provide an API for external system integrations beyond the built-in REST endpoints.

---

## 2. Architecture Overview

### 2.1 Technology Stack

| Layer      | Technology                                |
|------------|-------------------------------------------|
| Backend    | .NET 10, ASP.NET Core Web API             |
| ORM        | Entity Framework Core 10                  |
| Database   | PostgreSQL                                |
| Auth       | ASP.NET Identity + JWT Bearer Tokens      |
| Validation | FluentValidation                          |
| PDF Engine | QuestPDF (Community License)              |
| Frontend   | React 19, Vite 7, Ant Design 5           |
| HTTP       | Axios with JWT Interceptor                |
| Mapping    | AutoMapper                                |

### 2.2 Architecture Diagram

```
┌──────────────────────────────────────────────────────┐
│                    React Frontend                     │
│    (Vite Dev Server – port 5173)                     │
│  ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐  │
│  │  Login   │ │ Employee │ │ Employee │ │Employee │  │
│  │  Page    │ │   List   │ │   Form   │ │ Detail  │  │
│  └─────────┘ └──────────┘ └──────────┘ └─────────┘  │
│         │  Axios + JWT Bearer Token  │               │
└─────────┼────────────────────────────┼───────────────┘
          │        /api/* proxy        │
┌─────────▼────────────────────────────▼───────────────┐
│              ASP.NET Core Web API                     │
│              (port 5147)                              │
│  ┌──────────┐ ┌───────────────┐ ┌─────────────────┐  │
│  │   Auth   │ │   Employees   │ │   PDF Service   │  │
│  │Controller│ │  Controller   │ │   (QuestPDF)    │  │
│  └──────────┘ └───────────────┘ └─────────────────┘  │
│         │    EF Core + Identity    │                  │
└─────────┼──────────────────────────┼─────────────────┘
          │                          │
┌─────────▼──────────────────────────▼─────────────────┐
│                   PostgreSQL                          │
│  ┌───────────┐ ┌────────┐ ┌──────────┐ ┌──────────┐  │
│  │ Employees │ │Spouses │ │ Children │ │Identity  │  │
│  │           │ │        │ │          │ │ Tables   │  │
│  └───────────┘ └────────┘ └──────────┘ └──────────┘  │
└──────────────────────────────────────────────────────┘
```

---

## 3. Entity Relationship Diagram (ERD)

```
┌──────────────────┐       1:1       ┌──────────────────┐
│    Employee       │───────────────▶│     Spouse        │
├──────────────────┤                 ├──────────────────┤
│ Id (PK)          │                 │ Id (PK)          │
│ Name             │                 │ Name             │
│ NID (Unique)     │                 │ NID (Unique)     │
│ Phone            │                 │ EmployeeId (FK)  │
│ Department       │                 └──────────────────┘
│ BasicSalary      │
└──────────────────┘
        │
        │ 1:N
        ▼
┌──────────────────┐
│     Child         │
├──────────────────┤
│ Id (PK)          │
│ Name             │
│ DateOfBirth      │
│ EmployeeId (FK)  │
└──────────────────┘
```

**Relationships:**
- `Employee` ↔ `Spouse`: One-to-One (an employee can have at most one spouse)
- `Employee` ↔ `Child`: One-to-Many (an employee can have zero or more children)
- Cascade delete: Removing an employee also removes their spouse and children records.

---

## 4. Edge Cases & Handling

### 4.1 NID Validation
| Scenario | Handling |
|---|---|
| NID not 10 or 17 digits | Rejected by FluentValidation with clear error message |
| NID contains non-numeric characters | Rejected (regex: `^\d+$`) |
| Duplicate Employee NID | Returns HTTP 409 Conflict with message "An employee with this NID already exists" |
| Duplicate Spouse NID | Returns HTTP 409 Conflict — checked across all spouses |
| Same NID on update (own record) | Allowed — uniqueness check excludes the current employee's ID |

### 4.2 Phone Number Validation
| Scenario | Handling |
|---|---|
| `+880` format | Must be `+880` followed by exactly 10 digits (14 chars total) |
| `01` format | Must be `01` followed by exactly 9 digits (11 chars total) |
| Other prefixes | Rejected with "Must be valid BD format" error |

### 4.3 Family Data Edge Cases
| Scenario | Handling |
|---|---|
| Employee without a spouse | Spouse is nullable; stored as `null` in JSON response |
| Employee without children | Empty array `[]` returned |
| Removing spouse during update | If spouse was `null` in update payload, existing spouse record is deleted |
| Updating children | All existing children are replaced with the new list on each update |

### 4.4 Authentication & Authorization
| Scenario | Handling |
|---|---|
| Invalid credentials | Returns HTTP 401 with "Invalid username or password" |
| Expired JWT token | Returns HTTP 401; frontend clears token and redirects to login |
| Viewer trying to create/edit/delete | Returns HTTP 403; UI hides the buttons for non-admin users |
| Invalid registration data | Returns HTTP 400 with specific Identity errors (weak password, duplicate username) |

### 4.5 Search Behavior
| Scenario | Handling |
|---|---|
| Empty search | Returns all employees |
| Partial match | Case-insensitive `LIKE` on Name, NID, and Department |
| No results | Returns empty array; table shows empty state |
| Search debounce | Frontend debounces input by 300ms to avoid excessive API calls |

---

## 5. Assumptions

1. **One Spouse Maximum**: An employee can have at most one spouse record. This is enforced by the one-to-one relationship in the database.
2. **No Upper Limit on Children**: There is no enforced maximum number of children per employee.
3. **NID is Required for Spouse**: If a spouse is added, both Name and NID are mandatory.
4. **Children Replace Strategy**: On employee update, all existing children records are deleted and replaced with the new list from the request. This simplifies the API but means child IDs may change between updates.
5. **Self-Registration**: Any user can register with either Admin or Viewer role. In a production environment, Admin creation should be restricted.
6. **Single Currency**: All salary values are assumed to be in Bangladeshi Taka (BDT).
7. **No Time Zone Issues**: Dates (child DOB) are stored as UTC. Display formatting is handled by the frontend.
8. **Database Seeding**: The system automatically seeds 10 employees with realistic Bangladeshi names on first run if the database is empty.
9. **No Pagination on Backend**: The API returns all matching employees. For a production system with thousands of records, server-side pagination would be needed.
10. **PDF Generation is Server-Side**: PDFs are generated on the .NET backend using QuestPDF and streamed to the client as downloadable files.

---

## 6. API Endpoints Summary

| Method | Endpoint | Auth | Role | Description |
|--------|---------|------|------|-------------|
| POST | `/api/auth/login` | — | — | Login, returns JWT |
| POST | `/api/auth/register` | — | — | Register new user |
| GET | `/api/employees` | JWT | Any | List all (with `?search=` filter) |
| GET | `/api/employees/{id}` | JWT | Any | Get single employee |
| POST | `/api/employees` | JWT | Admin | Create employee |
| PUT | `/api/employees/{id}` | JWT | Admin | Update employee |
| DELETE | `/api/employees/{id}` | JWT | Admin | Delete employee |
| GET | `/api/employees/export/pdf` | JWT | Any | Download table PDF |
| GET | `/api/employees/{id}/export/cv` | JWT | Any | Download individual CV PDF |
