# 🚀 Task Management System (Monorepo Workspace)

This repository contains the full enterprise-grade solution for the Task Management System, designed under a unified **Monorepo** structure that encapsulates both the Backend API services and the Frontend User Interface application.

The platform implements a domain-driven architectural blueprint following **Clean Architecture** guidelines, strict **CQS (Command Query Separation)** principles, and a high-availability NoSQL persistence perimetral boundary.

---

## 🛠️ Technological Stack

### 🖥️ Backend (Core Engine)
- **Runtime:** .NET Core 8.0 LTS
- **Persistence Layer:** MongoDB (Official C# Driver)
- **Perimetral Request Validation:** FluentValidation (Fail-Fast structural design encapsulated inside UseCase Handlers)
- **Security & Cryptography:** Custom Cryptographic Password Hasher and JSON Web Token (JWT) Digital Signatures
- **Unit Testing Suite:** xUnit + Moq (Perimetral coverage mirroring Handlers and Controllers with absolute mock-isolation)

### 🎨 Frontend (User Interface)
- **Framework:** Angular 16.2.x (with Angular CLI 16.2.16)
- **Runtime Workspace:** Node.js v16.20.2 & npm 8.19.4
- **UI Styling Engine:** **Tailwind CSS** (Engineered using a utility-first clean architectural approach, guaranteeing a highly responsive layout, seamless interactive visual states, and cohesive component definitions without the technical debt of legacy CSS files)
- **State Management:** Reactive streams (RxJS) aligned with the User Interface lifecycle

### 🏗️ Architectural Approach (CQS Real-World Pattern)
Unlike typical CRUD architectures or heavy third-party framework reliance (such as MediatR), this solution enforces a lightweight, high-performance **CQS (Command Query Separation)** implementation mapped via native reflection scanning:
- **Commands:** Mutate state, execute asynchronous workflows, return pure business data wrappers, and utilize native, immutable C# `records` as Data Transfer Objects (DTOs).
- **Queries:** Retrieve data cleanly using streamlined database projections with zero side effects.
- **Isolated Handlers:** Single-responsibility processing units that encapsulate fail-fast structural data validation and business domain logic boundaries.

---

## 🔑 Demo Credentials (Database Seeding)

To streamline the evaluation process for the technical jury, the application includes an idempotent **Database Seeding** mechanism. Upon Backend bootstrap, if the database collections are determined to be virgin/empty, the system automatically injects the following mocked security profiles with cryptographically secure hashed passwords:

| Username | Password | Context / Simulated Demo Scenario |
| :--- | :--- | :--- |
| **`user_alpha`** | `Password123!` | Historical user account (Created 5 days ago). Ideal for inspecting a fully loaded task board and historical metrics. |
| **`user_beta`** | `Password123!` | Active user account (Created 3 days ago) containing mid-frequency data. |
| **`user_gamma`** | `Password123!` | Freshly created account (Created 1 day ago) for testing clean slate initial states. |

---

## 💻 Prerequisites

Before deploying the solution locally, ensure your machine has the following tools installed:
1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
2. **[Node.js v16.x LTS](https://nodejs.org/)** (Matching workspace version 16.20.2)
3. **[Angular CLI v16.x](https://angular.io/)** (Globally or handled locally via npm scripts)
4. **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (Mandatory prerequisite to orchestrate the containerized MongoDB environment seamlessly without demanding local database server installations)

---

## 🚀 Step-by-Step Setup & Deployment Guide

Execute the following commands in order from your terminal to build, validate, and launch the entire monorepo:

### 1. Orchestrate Infrastructure via Docker
Open your terminal at the repository root and spin up the isolated, background-detached MongoDB container instance:

```bash
docker-compose up -d
