# 🚀 Task Management System (Monorepo Workspace)

This repository contains the full enterprise-grade solution for the Task Management System, designed under a unified **Monorepo** structure that encapsulates both the Backend API services and the Frontend User Interface application.

The platform implements a domain-driven architectural blueprint following **Clean Architecture** guidelines, strict **CQS (Command Query Separation)** principles, and a high-availability NoSQL persistence perimetral boundary.

---

## 📖 Informal User Story

Following the assignment guidelines outlined in the Technical Interview Exercise, the design and development of this system were driven by a core user requirement mapped into the following scenario:

> **As a** Project Collaborator,  
> **I want to** authenticate securely into my personal dashboard and manage my daily responsibilities (creating, reading, updating, and removing tasks),  
> **So that** I can maintain absolute focus on my active deliverables and ensure strict multi-tenant data isolation from other team members.

### 📝 Functional Acceptance Criteria

- **Multi-Tenant Isolation:** A logged-in user can only execute CRUD actions on tasks linked directly to their own account. Accessing, editing, or mutating another user's records is strictly prohibited.
- **Task Attributes Validation:** Every task must enforce a non-empty Title, a detailed Description, a defined Status (`Pending`, `In Progress`, `Completed`), and a valid Due Date.
- **Cryptographic Guardrails:** The system must validate user presence through JSON Web Tokens (JWT) for all authorized endpoint operations.

---

## 🛠️ Technological Stack

### 🖥️ Backend (Core Engine)

- **Runtime:** .NET Core 8.0 LTS
- **Persistence Layer:** MongoDB (Official C# Driver)
- **Perimetral Request Validation:** FluentValidation (Fail-Fast structural design encapsulated inside UseCase Handlers)
- **Security & Cryptography:** Custom Cryptographic Password Hasher and JSON Web Token (JWT) Digital Signatures
- **Unit Testing Suite:** xUnit + Moq (Perimetral coverage mirroring Handlers and Controllers with absolute mock-isolation)

### 🎨 Frontend (User Interface)

- **Framework:** Angular 16.2.x (built with Angular CLI 16.2.16)
- **Runtime Workspace:** Node.js v16.20.2 & npm 8.19.4
- **UI Styling Engine:** **Tailwind CSS** (Engineered using a utility-first clean architectural approach, guaranteeing a highly responsive layout, seamless interactive visual states, and cohesive component definitions without the technical debt of legacy CSS files)
- **State Management:** Reactive streams (RxJS) aligned with the User Interface lifecycle

### 🏗️ Architectural Approach (CQS Real-World Pattern)

Unlike typical CRUD architectures or heavy third-party framework reliance (such as MediatR), this solution enforces a lightweight, high-performance **CQS (Command Query Separation)** implementation mapped via native reflection scanning:

- **Commands:** Mutate state, execute asynchronous workflows, return pure business data wrappers, and utilize native, immutable C# records as Data Transfer Objects (DTOs).
- **Queries:** Retrieve data cleanly using streamlined database projections with zero side effects.
- **Isolated Handlers:** Single-responsibility processing units that encapsulate fail-fast structural data validation and business domain logic boundaries.

---

## 🔑 Demo Credentials (Database Seeding)

To streamline the evaluation process, the application includes an idempotent **Database Seeding** mechanism. Upon Backend bootstrap, if the database collections are determined to be empty, the system automatically runs a seeding routine to generate three distinct security profiles.

- **Universal Seeding Password:** `Password123!`

| Username | Context / Simulated Demo Scenario |
| :--- | :--- |
| **`user_alpha`** | Historical user account created 5 days ago. Ideal for inspecting a fully loaded task board and historical metrics. |
| **`user_beta`** | Active user account created 3 days ago containing mid-frequency data. |
| **`user_gamma`** | Freshly created account (1 day ago) for testing clean slate initial states. |

---

## 💻 Prerequisites

Before deploying the solution locally, ensure your machine has the following tools installed:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
2. **[Node.js v16.x LTS](https://nodejs.org/)** — matching workspace version `16.20.2`
3. **[Angular CLI v16.x](https://angular.io/)** — install globally via `npm install -g @angular/cli@16`
4. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** — mandatory to orchestrate the containerized MongoDB environment

---

## 🚀 Step-by-Step Setup & Deployment Guide

Execute the following commands in order from your terminal to build, validate, and launch the entire monorepo.

### 1. Orchestrate Infrastructure via Docker

Spin up the isolated MongoDB container in detached background mode:

```bash
docker-compose up -d
```

### 2. Launch the Backend API

Navigate to the backend project directory and start the .NET API:

```bash
cd src/backend
dotnet restore
dotnet run
```

The API will be available by default at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

> ⚠️ On first run, the **Database Seeding** mechanism will automatically populate the three demo user profiles listed above.

### 3. Run Backend Unit Tests

From the backend project root, execute the full xUnit test suite:

```bash
dotnet test
```

### 4. Launch the Frontend UI

Navigate to the Angular application directory, install dependencies, and start the development server.

#### Using the Terminal (any OS):

```bash
cd src/frontend
npm install
ng serve
```

#### Using Visual Studio (Package Manager Console or Task Runner):

Open the **Package Manager Console** or configure a **Task Runner** entry in Visual Studio and execute:

```bash
ng serve
```

Once compiled successfully, the UI will be accessible at:

**➡️ [http://localhost:4200](http://localhost:4200)**

> The Angular development server supports **live reload** — any changes to source files will automatically refresh the browser.

#### Optional flags for `ng serve`:

| Flag | Description |
| :--- | :--- |
| `--open` | Automatically opens the browser upon startup |
| `--port 4201` | Runs the server on a custom port if `4200` is already in use |

Example with flags:

```bash
ng serve --open --port 4200
```

---


## 🔐 API Endpoints Overview

### Auth API (`/api/auth`)

| Method | Endpoint | Auth Required | Description |
| :--- | :--- | :---: | :--- |
| `POST` | `/api/auth/register` | ❌ | Create a new user account |
| `POST` | `/api/auth/login` | ❌ | Authenticate and receive a JWT token |

### Tasks API (`/api/tasks`)

| Method | Endpoint | Auth Required | Description |
| :--- | :--- | :---: | :--- |
| `GET` | `/api/tasks` | ✅ | Retrieve all tasks for the authenticated user |
| `GET` | `/api/tasks/{id}` | ✅ | Retrieve a specific task by ID |
| `POST` | `/api/tasks` | ✅ | Create a new task |
| `PUT` | `/api/tasks/{id}` | ✅ | Update an existing task |
| `DELETE` | `/api/tasks/{id}` | ✅ | Delete a task |

> All protected endpoints require the `Authorization: Bearer <token>` header obtained from the login response.

---

## 🤖 GenAI Tools — Prompt Engineering & AI-Assisted Development

In alignment with the technical exercise requirements, this section documents the use of Generative AI tools throughout the development of this project.

### Prompt Used to Scaffold the RESTful API

The following prompt was used with **Claude** (Anthropic) to generate the initial API scaffold:

```
You are an expert .NET 8 backend engineer. Generate a RESTful Web API for a Task Management 
System following Clean Architecture principles with CQS (Command Query Separation) pattern 
WITHOUT using MediatR, Entity Framework, or Dapper.

Requirements:
- Runtime: .NET 8 / ASP.NET Core Web API
- Database: MongoDB using the official C# driver
- Authentication: JWT Bearer tokens with a custom password hasher (no Identity framework)
- Entities: User (Id, Username, PasswordHash, CreatedAt) and Task (Id, Title, Description, 
  Status [Pending|InProgress|Completed], DueDate, UserId, CreatedAt)
- Architecture layers: Api / Application (Commands + Queries + Handlers) / Domain / Infrastructure
- Validation: FluentValidation inside each Handler (fail-fast approach)
- Multi-tenant isolation: every task query must be scoped to the authenticated UserId from the JWT claims
- Seeding: idempotent seed on startup for 3 demo users with Password123!

Generate: folder structure, domain entities as C# records, IRepository interfaces, MongoDB 
implementations, CQS Handlers, Controllers, JWT service, and DI registration in Program.cs.
```

### How the AI Output Was Validated

**Structural review:** The generated layer boundaries were verified against Clean Architecture dependency rules — outer layers (Infrastructure, Api) depend inward (Application, Domain), never the reverse.

**Security audit:** JWT token expiration, claim extraction for multi-tenancy, and password hashing were manually inspected. The AI initially omitted `ClockSkew = TimeSpan.Zero` on JWT validation parameters, which was added to prevent token replay windows.

**Edge cases handled:** The AI did not initially cover the scenario where a user attempts to access another user's task by ID (IDOR vulnerability). An ownership check (`task.UserId != currentUserId → 403 Forbidden`) was added manually to the `GetTaskByIdHandler` and `UpdateTaskHandler`.

**Validation gaps corrected:** FluentValidation rules for `DueDate` (must be a future date) and `Status` (must be a valid enum value) were absent in the initial output and added manually.

**Test coverage:** The AI-generated unit tests used `Moq` correctly for repository interfaces but lacked negative test cases (e.g., unauthorized access, not-found scenarios). These were added to achieve meaningful coverage.

---

## 📋 Evaluation Criteria Compliance

| Criterion | Implementation |
| :--- | :--- |
| **Clean Architecture** | Strict layer separation: Domain → Application → Infrastructure → Api |
| **TDD / Unit Tests** | xUnit + Moq covering Handlers, Repositories, and Controllers |
| **Code Quality** | C# records as immutable DTOs, single-responsibility Handlers, no magic strings |
| **Functionality** | Full CRUD with JWT auth, multi-tenant isolation, seeded demo data |
| **No Forbidden Libs** | Zero usage of Entity Framework, Dapper, or MediatR |
| **Frontend** | Angular 16 + Tailwind CSS, responsive, RxJS state management |
| **GenAI Fluency** | Documented prompt, validated output, and manually corrected edge cases |
