
# 🛒 E-Commerce Microservices Architecture — .NET 10, CQRS & Event-Driven

> **A resilient, highly scalable hybrid CQRS ecosystem engineered to guarantee consistency and performance at enterprise scale.**

---

## 📋 Table of Contents

- [System Overview](#system-overview)
- [Architecture Blueprint](#architecture-blueprint)
- [The Foundational Paradigm: Strict CQRS](#the-foundational-paradigm-strict-cqrs)
- [.NET 10 Technical Stack](#net-10-technical-stack)
- [Edge Layer Routing](#edge-layer-routing)
- [Endpoint Routing Matrix](#endpoint-routing-matrix)
- [Security & Validation Pipeline](#security--validation-pipeline)
- [Domain Boundary Map](#domain-boundary-map)
- [Authentication & Authorization Flow](#authentication--authorization-flow)
- [CQRS in Practice: Product & Catalog](#cqrs-in-practice-product--catalog)
- [Event-Driven Asynchronous Utilities: Email Service](#event-driven-asynchronous-utilities-email-service)
- [Distributed Checkout Orchestration](#distributed-checkout-orchestration)
- [Order Lifecycle & External Payment](#order-lifecycle--external-payment)
- [Internal Communication Mesh](#internal-communication-mesh)
- [Guaranteeing Eventual Consistency](#guaranteeing-eventual-consistency)
- [Microservices Functional Requirements](#microservices-functional-requirements)
- [API Gateways (Edge Layer)](#api-gateways-edge-layer)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [References & Further Reading](#references--further-reading)

---

## System Overview

This is a distributed e-commerce platform built on a **microservices architecture** that strictly separates read and write operations at the edge by utilizing two distinct API Gateways:

- **RESTful Gateway** → Commands (state mutations)
- **GraphQL Gateway** → Queries (data retrieval)

The backend consists of **six specialized microservices** communicating via:
- **Asynchronous** event-driven messaging (RabbitMQ)
- **Synchronous** gRPC calls for high-performance internal operations

> 🔗 **Full Repository:** [github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete](https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete)

---

## Architecture Blueprint

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENT LAYER                                    │
│                         (Web / Mobile / Admin Portal)                        │
└──────────────────────┬─────────────────────────────┬────────────────────────┘
                       │                             │
              ┌────────▼─────────┐         ┌────────▼─────────┐
              │  Command Gateway │         │   Query Gateway   │
              │   (YARP / REST)  │         │    (GraphQL)      │
              └────────┬─────────┘         └────────┬─────────┘
                       │                             │
              ┌────────▼───────────────────────────▼─────────┐
              │              INTERNAL MESH                      │
              │  ┌─────────┐  ┌─────────┐  ┌─────────┐       │
              │  │ Product │  │  Cart   │  │  Order  │       │
              │  │ Service │  │ Service │  │ Service │       │
              │  └────┬────┘  └────┬────┘  └────┬────┘       │
              │       │            │            │            │
              │  ┌────▼────┐  ┌────▼────┐  ┌────▼────┐       │
              │  │ Payment │  │  Email  │  │   Auth  │       │
              │  │ Service │  │ Service │  │ Service │       │
              │  └────┬────┘  └────┬────┘  └────┬────┘       │
              └───────┼────────────┼────────────┼────────────┘
                      │            │            │
              ┌───────▼────────────▼────────────▼────────────┐
              │           DATA & INFRASTRUCTURE                │
              │  SQL Server  │  RabbitMQ  │  EF Core / LINQ    │
              └────────────────────────────────────────────────┘
```

---

## The Foundational Paradigm: Strict CQRS

> **Highly scalable e-commerce infrastructure built on strict Command Query Responsibility Segregation (CQRS).**

The architecture enforces a **strict separation of reads and writes** from the edge layer all the way to the database:

| Aspect | Command Side (Writes) | Query Side (Reads) |
|--------|----------------------|---------------------|
| **Gateway** | YARP Reverse Proxy (RESTful) | GraphQL Subgraphs |
| **HTTP Methods** | POST / PUT / DELETE | GET (via GraphQL) |
| **Responsibility** | State mutations, business logic triggers, rate limiting | Data aggregation, projection, pagination |
| **Routing** | `/api/v1/commands/{domain}` | `/graphql` |
| **Database** | Write-Optimized (SQL Server / EF Core) | Read-Optimized projections |

### Visual Separation

```
┌─────────────┐     State Mutations      ┌──────────────────┐
│   Client    │ ────────────────────────▶ │  Command Gateway │
│ (Web/Mobile)│                          │   (YARP/REST)    │
└─────────────┘                          └──────────────────┘
       │
       │ Data Retrieval
       ▼
┌──────────────────┐
│   Query Gateway  │
│    (GraphQL)     │
└──────────────────┘
```

---

## .NET 10 Technical Stack

### Layered Architecture

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Edge Layer** | YARP (Reverse Proxy), GraphQL | Entry point routing & aggregation |
| **Application & Execution** | Wolverine (CQRS Message Routing), Clean Architecture | Core business logic & message handling |
| **Tooling** | FluentValidation, Mapperly, FluentResult, Serilog | Validation, mapping, result handling, logging |
| **Internal Communication** | gRPC (Synchronous), RabbitMQ (Asynchronous) | Service-to-service communication |
| **Data & Infrastructure** | EF Core, SQL Server, Repository Pattern, LINQ | Data persistence & access |

### Wolverine: Core Engine

> **Wolverine** serves as the core engine for CQRS message routing and execution, replacing traditional MediatR approaches with a more performant, native .NET messaging framework.

---

## Edge Layer Routing

### Command vs. Query Gateways

```
                    ┌─────────────┐
                    │Client Request│
                    └──────┬──────┘
                           │
                    ┌──────▼──────┐
                    │ Routing Fork │
                    └──────┬──────┘
           ┌───────────────┼───────────────┐
           ▼               │               ▼
┌─────────────────────┐   │   ┌─────────────────────┐
│  Command Gateway    │   │   │   Query Gateway     │
│    (RESTful)        │   │   │    (GraphQL)        │
├─────────────────────┤   │   ├─────────────────────┤
│ Technology: YARP    │   │   │ Technology: Unified   │
│                     │   │   │ GraphQL Subgraphs     │
│ Responsibility:     │   │   │                     │
│ • State mutations   │   │   │ Responsibility:     │
│ • Business logic    │   │   │ • Data aggregation  │
│ • Rate limiting     │   │   │ • Projection        │
│                     │   │   │ • Pagination        │
│ Routing:            │   │   │                     │
│ POST/PUT/DELETE to  │   │   │ Routing:            │
│ isolated downstream │   │   │ Unified entry point │
│ domains             │   │   │ for complex data    │
│                     │   │   │ retrieval           │
└─────────────────────┘   │   └─────────────────────┘
                          │
           ┌──────────────▼──────────────┐
           │   Primary Security Boundary  │
           │  Shared JWT verification:    │
           │  Key, Issuer, Audience       │
           └──────────────────────────────┘
```

---

## Endpoint Routing Matrix

### Command Executions (REST)

| Method | Endpoint | Action |
|--------|----------|--------|
| `POST` | `/api/v1/commands/product` | Create product |
| `PUT` | `/api/v1/commands/cart` | Update cart |
| `DELETE` | `/api/v1/commands/order` | Delete order |

> All commands flow through **Wolverine Execution Queues** for reliable processing.

### Query Retrievals (GraphQL)

```graphql
POST /graphql
{
  products {
    id
    name
    price
  }
}
```

> Queries are resolved through **Unified Catalog Data Aggregation**, eliminating over-fetching.

---

## Security & Validation Pipeline

### Two-Tier Security Boundary

```
┌─────────────────┐    ┌─────────────────────┐    ┌─────────────────────────┐
│  Edge Layer     │───▶│ Downstream Service  │───▶│  Execution & Data Layer │
│  (Gateways)     │    │      Layer          │    │                         │
├─────────────────┤    ├─────────────────────┤    ├─────────────────────────┤
│ Preliminary     │    │ Fine-Grained Auth:  │    │ Standardization:        │
│ Check:          │    │ Validates specific  │    │ FluentValidation &      │
│ Validates Key,  │    │ Roles and User IDs  │    │ FluentResult handle     │
│ Issuer, and     │    │ locally.            │    │ structured              │
│ Audience via    │    │                     │    │ success/failure         │
│ JWT.            │    │                     │    │ responses before EF     │
│                 │    │                     │    │ Core execution.         │
└─────────────────┘    └─────────────────────┘    └─────────────────────────┘
```

**Gateways reject invalid tokens immediately.** Authorized requests proceed to domain-specific role validation.

---

## Domain Boundary Map

### Core vs. Utility Topologies

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Zone A (60%)                          │ Zone B (40%)                         │
│ Core Services (Clean Architecture)    │ Utility Services (Minimal APIs)      │
│ Full domain logic, Application, API,  │ Lightweight execution and background │
│ and Infrastructure layers.            │ processing pipelines.                │
│                                       │                                      │
│  ┌─────────────┐  ┌─────────────┐     │  ┌─────────────┐                     │
│  │   Product   │  │    Cart     │     │  │   Payment   │ Query/Read,        │
│  │   Service   │◀─┤   Service   │────▶│  │   Service   │ integration        │
│  │  (Data/     │  │  (Cart/     │     │  └─────────────┘                     │
│  │   Logic)    │  │   State)    │     │                                      │
│  └─────────────┘  └──────┬──────┘     │  ┌─────────────┐                     │
│                          │            │  │    Email    │ Asynchronous       │
│  ┌─────────────┐         │            │  │   Service   │ messaging            │
│  │    Order    │◀────────┘            │  └─────────────┘                     │
│  │   Service   │                      │                                      │
│  │  (History)  │                      │  ┌─────────────┐                     │
│  └─────────────┘                      │  │    Auth     │ Synchronous        │
│                                       │  │   Service   │ internal gRPC        │
│                                       │  └─────────────┘                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Authentication & Authorization Flow

```
┌────────┐   Registration Request    ┌────────────────┐
│ Client │ ────────────────────────▶ │ Command Gateway│
└────────┘                         └───────┬────────┘
                                           │
                                           │ Route to Auth Domain
                                           ▼
                                    ┌──────────────┐
                                    │ Auth Service │
                                    └──────┬───────┘
                                           │
                    ┌──────────────────────┼──────────────────────┐
                    │                      │                      │
                    ▼                      ▼                      ▼
            ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
            │ Publish OTP │      │  Login Request│      │Return Access│
            │   Event     │      │             │      │+ Refresh    │
            └──────┬──────┘      └──────┬──────┘      │   Tokens    │
                   │                      │             └─────────────┘
                   ▼                      │
            ┌─────────────┐             │
            │  RabbitMQ   │             │
            └──────┬──────┘             │
                   │                      │
                   ▼                      │
            ┌─────────────┐             │
            │Consume OTP  │             │
            │   Event     │             │
            └──────┬──────┘             │
                   │                      │
                   ▼                      │
            ┌─────────────┐             │
            │Email Service│◀────────────┘
            │Dispatch Email│
            └──────┬──────┘
                   │
                   ▼
            ┌─────────────┐
            │  User Inbox  │
            └─────────────┘
```

**Trigger Mechanism:** Automatic refresh token generation is specifically triggered when updating authorization roles/permissions of an endpoint or a user.

---

## CQRS in Practice: Product & Catalog

> Admin mutations are strictly controlled via REST. Customers browse seamlessly via optimized GraphQL projection.

```
┌───────────┐     ┌──────────────┐     ┌─────────────────────────┐
│Admin User │────▶│ Command      │────▶│ RESTful Commands        │
│           │     │ Gateway      │     │ (Create, Update, Delete)│
└───────────┘     └──────────────┘     └─────────────┬───────────┘
                                                     │
                                                     │ Mutates Inventory State
                                                     ▼
                                           ┌─────────────────────┐
                                           │  Product Catalog    │
                                           │   Database          │
                                           │ (SQL Server / EF)  │
                                           └──────────┬──────────┘
                                                      │
                                                      │ High-Performance Retrieval
                                                      ▼
┌───────────┐     ┌──────────────┐     ┌─────────────────────────┐
│ Customer  │────▶│ Query        │────▶│ Unified GraphQL          │
│           │     │ Gateway      │     │ (Get All, Get By ID)     │
└───────────┘     └──────────────┘     └─────────────────────────┘
```

---

## Event-Driven Asynchronous Utilities: Email Service

> The Email service runs as a background minimal API, reacting to system events without impacting synchronous user-facing response times.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PUBLISHERS                                      │
│  ┌─────────────┐              ┌─────────────┐                               │
│  │ Auth Service│              │Order Service│                               │
│  └──────┬──────┘              └──────┬──────┘                               │
│         │                             │                                     │
│         └──────────────┬──────────────┘                                     │
│                        │                                                    │
│                        ▼                                                    │
│              ┌─────────────────┐                                            │
│              │  RabbitMQ       │                                            │
│              │ Message Broker  │                                            │
│              │                 │                                            │
│              │ ┌───────────┐ │                                            │
│              │ │ OTP Gen     │ │                                            │
│              │ │   Event     │ │                                            │
│              │ └───────────┘ │                                            │
│              │ ┌───────────┐ │                                            │
│              │ │Order Conf   │ │                                            │
│              │ │   Event     │ │                                            │
│              │ └───────────┘ │                                            │
│              └───────┬─────────┘                                            │
│                      │                                                      │
│                      ▼                                                      │
│              ┌─────────────────┐                                            │
│              │  Subscriber /   │                                            │
│              │Background Consumer│                                          │
│              │                 │                                            │
│              │  Email Service  │                                            │
│              └────────┬────────┘                                            │
│                       │                                                     │
│                       ▼                                                     │
│              ┌─────────────────┐                                            │
│              │   User Inbox    │                                            │
│              └─────────────────┘                                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Distributed Checkout Orchestration

> **The Symphony:** Cart Service acts as the orchestrator, coordinating a sequential saga across Product, Order, and Payment services.

```
┌────────┐   Initiate Checkout    ┌─────────────────────┐
│ Client │ ────────────────────▶ │  Cart Service       │
└────────┘                      │   (Orchestrator)    │
                                └──────────┬──────────┘
                                           │
           ┌───────────────────────────────┼───────────────────────────────┐
           │                               │                               │
           ▼                               ▼                               ▼
┌─────────────────────┐         ┌─────────────────────┐         ┌─────────────────────┐
│   Product Service   │         │   Order Service     │         │  Payment Service  │
│                     │         │                     │         │                     │
│ gRPC: Inventory     │         │ gRPC: Create Order  │         │ gRPC: Initiate    │
│     Check & Lock    │         │     Record          │         │     Transaction   │
│         │           │         │         │           │         │         │           │
│ Return Lock Status  │         │ Return Order ID     │         │ Return Payment    │
│         │           │         │         │           │         │     Token         │
│         └───────────┘         │         └───────────┘         │         └───────────┘
│                               │                               │
└───────────────────────────────┴───────────────────────────────┘
                                │
                                ▼
                         ┌─────────────┐
                         │Checkout      │
                         │Successful    │
                         └─────────────┘
```

---

## Order Lifecycle & External Payment

```
                    RESTful Order Cancellation Command
                              │
                              ▼
┌────────┐      ┌────────┐      ┌───────────┐      ┌──────────┐
│Pending │─────▶│  Paid  │─────▶│ Cancelled │─────▶│ Refunded │
└────────┘      └────┬───┘      └─────┬─────┘      └──────────┘
                     │                │
                     │                │ Synchronous/Asynchronous
                     │                │ Refund Initiation
                     │                │
                     ▼                ▼
              ┌─────────────────────────────┐
              │  Paymob Integration Gateway  │
              │      (Wallets & Cards)       │
              └─────────────────────────────┘
```

---

## Internal Communication Mesh

### Protocol Selection Strategy

| Protocol | Use Case | Characteristics |
|----------|----------|-----------------|
| **gRPC** | Synchronous, sequential flows | High-performance, strongly-typed, direct service-to-service |
| **RabbitMQ** | Background, fire-and-forget | Loose coupling, event triggers, prevents blocking user threads |

```
┌─────────────────────────────────┐  ┌─────────────────────────────────┐
│     Synchronous (gRPC)         │  │    Asynchronous (RabbitMQ)     │
│                                 │  │                                 │
│  ┌───────┐      gRPC      ┌────┴┐ │  ┌───────┐                     │
│  │Node A │ ─────────────▶ │NodeB│ │  │Node X │────────┐            │
│  └───────┘                └─────┘ │  └───────┘        │            │
│                                   │                   ▼            │
│  High-performance, strongly-typed  │              ┌─────────┐        │
│  direct service-to-service       │              │  Broker │        │
│  internal calls. Used heavily in   │              └────┬────┘        │
│  Cart orchestration.               │         ┌───────┼───────┐     │
│                                   │         ▼       ▼       ▼     │
│                                   │      ┌─────┐ ┌─────┐ ┌─────┐  │
│                                   │      │NodeY│ │NodeZ│ │ ... │  │
│                                   │      └─────┘ └─────┘ └─────┘  │
│                                   │                                 │
│                                   │  Loose coupling via event         │
│                                   │  triggers. Prevents blocking    │
│                                   │  user threads during background   │
│                                   │  processing.                    │
└─────────────────────────────────┘  └─────────────────────────────────┘
```

---

## Guaranteeing Eventual Consistency

> Addressing the distributed checkout challenge: Implementing the **Wolverine Outbox pattern** to guarantee eventual consistency and prevent data loss during cross-service orchestration.

### Wolverine Outbox Pattern vs. Dedicated Saga Orchestrator

```
┌─────────────────────────────────────────┐  ┌─────────────────────────────────┐
│    Wolverine Outbox Pattern             │  │   Dedicated Saga Orchestrator   │
│                                         │  │                                 │
│  ┌─────────┐                            │  │                                 │
│  │ Cart    │                            │  │  ┌─────────┐    ┌─────────┐   │
│  │ Service │                            │  │  │  Saga   │───▶│ Product │   │
│  └────┬────┘                            │  │  │ State   │    └────┬────┘   │
│       │                                 │  │  │ Machine │         │        │
│       ▼                                 │  │  └────┬────┘         │        │
│  ┌─────────────────────────┐            │  │       │              │        │
│  │      Local DB           │            │  │       │    Compensation        │
│  │   (Atomic Transaction)  │            │  │       │    / Rollback Logic   │
│  │                         │            │  │       │              │        │
│  │  ┌─────────────────┐    │            │  │       ▼              ▼        │
│  │  │  Save Cart      │    │            │  │  ┌─────────┐    ┌─────────┐   │
│  │  │    State        │    │            │  │  │  Order  │───▶│ Payment │   │
│  │  └─────────────────┘    │            │  │  └─────────┘    └─────────┘   │
│  │           │             │            │  │                                 │
│  │           ▼             │            │  └─────────────────────────────────┘
│  │  ┌─────────────────┐    │            │
│  │  │ Save Message    │    │            │
│  │  │   to Outbox     │    │            │
│  │  └─────────────────┘    │            │
│  │           │             │            │
│  │           ▼             │            │
│  │  ┌─────────────────┐    │            │
│  │  │ Async Publish   │───▶│ Message   │
│  │  │                 │    │  Broker   │
│  │  └─────────────────┘    │           │
│  └─────────────────────────┘            │
└─────────────────────────────────────────┘
```

**The system implements the Wolverine Outbox Pattern** as the primary mechanism for distributed transaction consistency, with Saga orchestration available for complex rollback scenarios.

---

## Microservices Functional Requirements

### 🔐 Auth Service
Manages identity, user provisioning, and token generation.

| Feature | Description |
|---------|-------------|
| **Registration** | Register new users; confirm email via OTP |
| **Authentication** | Login endpoint returning Access Token + Refresh Token |
| **Password Management** | Forgot password (Send OTP, Reset Password); Change password (requires active auth) |
| **Token Refresh** | Issue new access tokens; auto-refresh triggered on role/permission updates |

### 📦 Product Service
Manages the e-commerce catalog and inventory.

| Operation | Access | Protocol |
|-----------|--------|----------|
| Create, Update, Delete | Admin Only | RESTful Commands |
| Get All Products, Get By ID | Admin & Customer | GraphQL Queries |

### 🛒 Cart Service
Handles active shopping sessions before checkout.

| Feature | Description |
|---------|-------------|
| **Commands** | Add items, update quantities, remove items |
| **Queries** | Get active cart details (authorized customer) |
| **Checkout Orchestration** | Sequential saga: Product (inventory check/lock) → Order (record creation) → Payment (transaction initiation) |

### 📋 Order Service
Manages the lifecycle of customer purchases.

| Operation | Access | Protocol |
|-----------|--------|----------|
| Cancel Order | Authenticated | RESTful Command (triggers refund via Payment Service) |
| Get All Orders | Admin | GraphQL Query |
| Get My Orders | Customer | GraphQL Query |

### 💳 Payment Service
Handles external financial transactions.

| Feature | Description |
|---------|-------------|
| **Payment Processing** | Completes transactions via **Paymob integration** (local wallets & card gateways) |
| **Refunds** | Executes refunds synchronously or asynchronously |

### 📧 Email Service
An asynchronous, background utility service.

| Feature | Description |
|---------|-------------|
| **Event Consumption** | Listens to RabbitMQ queues for system events |
| **Notifications** | Dispatches emails for OTP generation, order confirmations, etc. |

---

## API Gateways (Edge Layer)

### Command Gateway (RESTful)

Acts as the reverse proxy for all write operations.

- **Routing:** YARP routes HTTP POST/PUT/DELETE to appropriate downstream services
- **Security:** Preliminary token verification (Key, Issuer, Audience)
- **Rate Limiting:** Enforced at gateway level
- **Authorization Delegation:** Fine-grained role/user validation delegated to downstream services

### Query Gateway (GraphQL)

Acts as the single point of entry for all read operations.

- **Data Aggregation:** Loads subgraphs/schemas from all services into unified API
- **Features:** Native filtering, sorting, pagination, projection
- **Optimization:** Eliminates over-fetching by returning only requested fields
- **Security:** Identical JWT verification as Command Gateway

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- [SQL Server](https://www.microsoft.com/sql-server) (or use Docker container)
- [RabbitMQ](https://www.rabbitmq.com/) (or use Docker container)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete.git
cd E-Commerce_Microservice_SystemDesign_Templete

# Start infrastructure services (SQL Server, RabbitMQ)
docker-compose -f docker-compose.infra.yml up -d

# Restore and build all services
dotnet restore
dotnet build

# Run individual services (or use launch profiles)
cd src/Services/AuthService && dotnet run
cd src/Services/ProductService && dotnet run
# ... etc
```

### Docker Deployment

```bash
# Build all service images
docker-compose build

# Start the complete stack
docker-compose up -d

# View logs
docker-compose logs -f
```

---

## Project Structure

```
E-Commerce_Microservice_SystemDesign_Templete/
├── src/
│   ├── Gateways/
│   │   ├── CommandGateway/          # YARP Reverse Proxy
│   │   └── QueryGateway/            # GraphQL API Gateway
│   ├── Services/
│   │   ├── AuthService/             # Identity & Token Management
│   │   ├── ProductService/          # Catalog & Inventory (Clean Arch)
│   │   ├── CartService/             # Shopping Session & Checkout Orchestrator
│   │   ├── OrderService/            # Purchase Lifecycle Management
│   │   ├── PaymentService/          # Paymob Integration
│   │   └── EmailService/            # Background Notification Processor
│   └── Shared/
│       ├── Contracts/               # gRPC Protos, Event Contracts
│       ├── BuildingBlocks/          # Common utilities, FluentResult, etc.
│       └── Infrastructure/          # EF Core configurations, Repository base
├── tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   └── ArchitectureTests/
├── docs/
│   ├── diagrams/                    # Architecture diagrams (PDF)
│   └── adr/                         # Architecture Decision Records
├── docker-compose.yml
├── docker-compose.infra.yml
└── README.md
```

---

## References & Further Reading

| Resource | Link |
|----------|------|
| **Main Repository** | [github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete](https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete) |
| Wolverine Documentation | [wolverine.net](https://wolverine.net) |
| YARP Reverse Proxy | [microsoft.github.io/reverse-proxy](https://microsoft.github.io/reverse-proxy/) |
| GraphQL .NET | [graphql-dotnet.github.io](https://graphql-dotnet.github.io/) |
| gRPC for .NET | [docs.microsoft.com/aspnet/core/grpc](https://docs.microsoft.com/aspnet/core/grpc) |
| RabbitMQ .NET Client | [rabbitmq.com/dotnet.html](https://www.rabbitmq.com/dotnet.html) |
| Clean Architecture | [github.com/jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture) |
| FluentValidation | [fluentvalidation.net](https://fluentvalidation.net/) |
| Paymob Integration | [paymob.com/docs](https://paymob.com/docs) |

---