# E-Commerce Microservices Architecture Template

A comprehensive, enterprise-grade E-Commerce platform built with a sophisticated microservices architecture using .NET 10. This template demonstrates best practices in distributed systems design, including event-driven architecture, CQRS pattern, dual gateway strategy, and advanced security mechanisms.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Microservices](#microservices)
- [API Gateways](#api-gateways)
- [Setup & Installation](#setup--installation)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## 🎯 Overview

This project provides a production-ready template for building scalable E-Commerce platforms using cutting-edge microservices patterns and technologies. It implements:

- **CQRS Pattern**: Complete separation of read and write operations
- **Event-Driven Architecture**: Asynchronous communication through message routing
- **Dual Gateway Strategy**: Optimized endpoints for different use cases
- **Clean Architecture**: Well-organized, maintainable code structure
- **Two-Tier Security**: Comprehensive validation and authorization layers
- **gRPC Support**: High-performance synchronous service-to-service communication
- **Distributed Tracing**: Full observability across services

### Key Highlights

✅ **Six Specialized Microservices** for complete e-commerce functionality  
✅ **Wolverine Message Routing** for intelligent CQRS implementation  
✅ **Dual Gateway Architecture** (YARP + GraphQL)  
✅ **RabbitMQ Event Bus** for reliable asynchronous communication  
✅ **gRPC Communication** for efficient inter-service calls  
✅ **Clean Architecture** following SOLID principles  
✅ **Enterprise-Grade Security** with multi-layer validation  
✅ **Docker & Kubernetes** ready deployment  

## 🏗️ Architecture

### System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         API Clients                               │
│              (Web, Mobile, Third-party Apps)                     │
└────────────────┬────────────────────────────────────────────────┘
                 │
        ┌────────┴────────┐
        │                 │
    ┌───▼────┐       ┌────▼────┐
    │  YARP  │       │ GraphQL  │
    │Gateway │       │ Gateway  │
    │(REST)  │       │(Queries) │
    └───┬────┘       └────┬─────┘
        │ Commands        │ Queries
        │                 │
┌───────┴─────────────────┴──────────────────────────────────┐
│                                                             │
│          Service Bus / Message Routing (Wolverine)         │
│                                                             │
└─────────┬────────────┬────────────┬────────────┬───────────┘
          │            │            │            │
    ┌─────▼──┐    ┌────▼──┐   ┌────▼──┐   ┌────▼──┐
    │Product │    │ Cart  │   │ Order │   │Payment│
    │Service │    │Service│   │Service│   │Service│
    └─────┬──┘    └────┬──┘   └────┬──┘   └────┬──┘
          │            │            │           │
    ┌─────▼──┐    ┌────▼──┐   ┌────▼──┐   ┌────▼──┐
    │  Auth  │    │ Email │   │ Event │   │Shared │
    │Service │    │Service│   │ Store │   │ Data  │
    └────────┘    └───────┘   └───────┘   └───────┘
          │
    ┌─────▼────────────────┐
    │    RabbitMQ          │
    │  (Event Broker)      │
    └──────────────────────┘
```

### CQRS & Event-Driven Flow

```
Commands (via YARP)
├── ProductCommand → ProductService → RabbitMQ (ProductCreated Event)
├── AddToCartCommand → CartService → RabbitMQ (ItemAddedToCart Event)
├── PlaceOrderCommand → OrderService → Payment/Email Services
└── ProcessPaymentCommand → PaymentService → RabbitMQ (PaymentProcessed Event)

Queries (via GraphQL)
├── GetProducts → ProductService (Read Model)
├── GetCartItems → CartService (Cached Read Model)
├── GetOrderHistory → OrderService (Event Store Query)
└── GetUserProfile → AuthService (Identity & Permissions)
```

### Two-Tier Security Architecture

```
┌──────────────────────────────────────┐
│   Tier 1: API Gateway Level          │
│   ├── Authentication (JWT/OAuth)     │
│   ├── Authorization (Role-based)     │
│   └── Rate Limiting                  │
└──────────────────────────────────────┘
            ↓
┌──────────────────────────────────────┐
│   Tier 2: Microservice Level         │
│   ├── Token Validation               │
│   ├── Permission Verification        │
│   ├── Data Authorization             │
│   └── Audit Logging                  │
└──────────────────────────────────────┘
```

## 💻 Tech Stack

### Core Framework
- **.NET 10** - Latest .NET runtime with performance improvements
- **ASP.NET Core** - Web framework for APIs and gateways
- **C# 13** - Modern language features and performance

### Architecture & Patterns
- **Wolverine** - Service bus and CQRS implementation
- **Clean Architecture** - Domain-driven design principles
- **CQRS Pattern** - Command Query Responsibility Segregation
- **Event Sourcing** - Complete event audit trail

### API Gateways
- **YARP** (Yet Another Reverse Proxy) - REST API gateway for commands
- **GraphQL** - Query gateway for efficient data fetching
- **Hotchocolate** - GraphQL server implementation

### Communication
- **gRPC** - High-performance RPC framework
- **RabbitMQ** - Message broker for asynchronous events
- **Protocol Buffers** - Efficient data serialization

### Database & Storage
- **SQL Server / PostgreSQL** - Primary data store
- **Redis** - Caching layer for read models
- **Event Store** - Dedicated event storage
- **Azure CosmosDB** - Optional distributed database

### Security
- **IdentityServer4** - OAuth2 / OpenID Connect server
- **JWT** - Token-based authentication
- **BCrypt** - Password hashing
- **OWASP** - Security best practices

### Observability
- **Serilog** - Structured logging
- **Datadog / New Relic** - APM (Application Performance Monitoring)
- **OpenTelemetry** - Distributed tracing
- **Prometheus** - Metrics collection

### Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking library
- **TestContainers** - Integration testing with containers
- **MassTransit** - Testing utilities for messaging

### DevOps & Deployment
- **Docker** - Containerization
- **Kubernetes** - Orchestration
- **Docker Compose** - Local development
- **Azure DevOps / GitHub Actions** - CI/CD pipelines

## 🔧 Microservices

### 1. **Auth Service** 🔐
Handles user authentication, authorization, and identity management.

**Responsibilities:**
- User registration and login
- JWT token generation and validation
- Role and permission management
- OAuth2/OpenID Connect integration
- Multi-factor authentication

**Technologies:** IdentityServer4, EF Core, SQL Server  
**Endpoints (gRPC):**
```
rpc ValidateToken(TokenRequest) returns (ValidationResponse);
rpc GetUserProfile(UserRequest) returns (UserProfile);
rpc RefreshToken(RefreshTokenRequest) returns (TokenResponse);
```

---

### 2. **Product Service** 📦
Manages product catalog, inventory, and product information.

**Responsibilities:**
- Product CRUD operations
- Inventory management
- Product categorization and search
- Stock level updates
- Price management

**Technologies:** EF Core, Redis caching, Full-text search  
**Commands:**
```
CreateProductCommand
UpdateProductCommand
DeleteProductCommand
AdjustInventoryCommand
```

**Queries:**
```
GetAllProductsQuery
GetProductByIdQuery
SearchProductsQuery
GetProductsByCategory
```

---

### 3. **Cart Service** 🛒
Manages shopping cart operations and cart persistence.

**Responsibilities:**
- Add/remove items from cart
- Update item quantities
- Cart total calculations
- Cart abandonment tracking
- Promotional code validation

**Technologies:** Redis, EF Core, Wolverine patterns  
**Commands:**
```
AddToCartCommand
RemoveFromCartCommand
UpdateCartItemCommand
ClearCartCommand
```

**Events:**
```
ItemAddedToCartEvent
ItemRemovedFromCartEvent
CartAbandonedEvent
```

---

### 4. **Order Service** 📋
Handles order management, fulfillment, and order history.

**Responsibilities:**
- Order creation and validation
- Order status tracking
- Order history management
- Shipment coordination
- Return and refund processing

**Technologies:** Event Sourcing, Event Store, Domain-driven design  
**Commands:**
```
PlaceOrderCommand
UpdateOrderStatusCommand
CancelOrderCommand
CreateReturnCommand
```

**Events:**
```
OrderCreatedEvent
OrderConfirmedEvent
OrderShippedEvent
OrderDeliveredEvent
OrderCancelledEvent
```

---

### 5. **Payment Service** 💳
Processes and manages payment transactions.

**Responsibilities:**
- Payment processing
- Transaction validation
- Payment gateway integration
- Refund handling
- Payment fraud detection

**Technologies:** Stripe/PayPal integration, Wolverine, gRPC  
**Commands:**
```
ProcessPaymentCommand
RefundPaymentCommand
ValidatePaymentMethod
```

**Events:**
```
PaymentProcessedEvent
PaymentFailedEvent
PaymentRefundedEvent
```

---

### 6. **Email Service** 📧
Handles email notifications and communication.

**Responsibilities:**
- Order confirmation emails
- Shipping notification emails
- User registration emails
- Password reset emails
- Marketing campaign emails

**Technologies:** SendGrid/SMTP, Hangfire for scheduling, Templates  
**Commands:**
```
SendOrderConfirmationCommand
SendShippingNotificationCommand
SendPasswordResetCommand
```

---

## 🌐 API Gateways

### YARP Gateway (Command Operations)

RESTful API gateway optimized for command operations using YARP (Yet Another Reverse Proxy).

**Base URL:** `https://api.yourapp.com`

**Key Routes:**
```
POST   /api/products              → Product Service (Create)
PUT    /api/products/{id}         → Product Service (Update)
DELETE /api/products/{id}         → Product Service (Delete)
POST   /api/cart/items            → Cart Service (Add Item)
DELETE /api/cart/items/{id}       → Cart Service (Remove Item)
POST   /api/orders                → Order Service (Create Order)
POST   /api/payments/process      → Payment Service (Process)
```

**Features:**
- Request routing and load balancing
- Authentication/Authorization middleware
- Rate limiting and throttling
- Request/Response transformation
- API versioning support

### GraphQL Gateway (Query Operations)

Advanced query gateway using GraphQL for efficient data fetching.

**Base URL:** `https://api.yourapp.com/graphql`

**Sample Query:**
```graphql
query GetOrderDetails {
  order(id: "order-123") {
    id
    total
    status
    items {
      product {
        name
        price
      }
      quantity
    }
    customer {
      email
      name
    }
  }
}
```

**Schema Coverage:**
- Products with filtering and pagination
- Cart operations
- Order history
- Customer profiles
- Payment history

**Advantages:**
- Single request for multiple data requirements
- Eliminates over-fetching and under-fetching
- Client-driven data requirements
- Built-in introspection and documentation

---

## 🚀 Setup & Installation

### Prerequisites

- **.NET 10 SDK** or later
- **Docker & Docker Compose**
- **RabbitMQ** (message broker)
- **SQL Server / PostgreSQL** (database)
- **Redis** (caching)
- **Git**

### Step 1: Clone the Repository

```bash
git clone https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete.git
cd E-Commerce_Microservice_SystemDesign_Templete
```

### Step 2: Environment Setup

Create a `.env` file in the root directory:

```env
# Database Configuration
DB_SERVER=localhost
DB_PORT=1433
DB_USERNAME=sa
DB_PASSWORD=YourPassword@123
DB_NAME=ECommerceDb

# RabbitMQ Configuration
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_VHOST=/

# Redis Configuration
REDIS_HOST=localhost
REDIS_PORT=6379

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-min-32-characters
JWT_ISSUER=https://auth.yourapp.com
JWT_AUDIENCE=ecommerce-api

# Service URLs
AUTH_SERVICE_URL=http://auth-service:5001
PRODUCT_SERVICE_URL=http://product-service:5002
CART_SERVICE_URL=http://cart-service:5003
ORDER_SERVICE_URL=http://order-service:5004
PAYMENT_SERVICE_URL=http://payment-service:5005
EMAIL_SERVICE_URL=http://email-service:5006
```

### Step 3: Docker Compose Setup

Run all infrastructure services:

```bash
docker-compose up -d
```

**Services started:**
- SQL Server on `localhost:1433`
- RabbitMQ on `localhost:5672` (Management UI: `localhost:15672`)
- Redis on `localhost:6379`

### Step 4: Build & Run Services

**Option A: Individual Service Startup**

```bash
# Terminal 1: Auth Service
cd src/Services/AuthService
dotnet run

# Terminal 2: Product Service
cd src/Services/ProductService
dotnet run

# Terminal 3: Cart Service
cd src/Services/CartService
dotnet run

# Terminal 4: Order Service
cd src/Services/OrderService
dotnet run

# Terminal 5: Payment Service
cd src/Services/PaymentService
dotnet run

# Terminal 6: Email Service
cd src/Services/EmailService
dotnet run

# Terminal 7: YARP Gateway
cd src/Gateways/YarpGateway
dotnet run

# Terminal 8: GraphQL Gateway
cd src/Gateways/GraphQLGateway
dotnet run
```

**Option B: Docker Compose with Services**

```bash
docker-compose -f docker-compose.services.yml up -d
```

### Step 5: Database Migration

```bash
cd src/Services/AuthService
dotnet ef database update

cd ../ProductService
dotnet ef database update

cd ../CartService
dotnet ef database update

cd ../OrderService
dotnet ef database update

cd ../PaymentService
dotnet ef database update
```

### Step 6: Verify Installation

```bash
# Check YARP Gateway
curl http://localhost:5000/health

# Check GraphQL Gateway
curl http://localhost:5001/graphql/health

# Check Auth Service
curl http://localhost:5002/health
```

---

## 📚 Usage Examples

### 1. User Registration & Authentication

**Register User:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword@123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response:**
```json
{
  "id": "user-123",
  "email": "user@example.com",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "RefreshToken123...",
  "expiresIn": 3600
}
```

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword@123"
  }'
```

---

### 2. Product Management

**Create Product:**
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Wireless Headphones",
    "description": "Premium noise-cancelling headphones",
    "price": 199.99,
    "stock": 100,
    "category": "Electronics"
  }'
```

**Get All Products (GraphQL):**
```graphql
query {
  products(first: 10, skip: 0) {
    edges {
      node {
        id
        name
        price
        stock
        category
      }
    }
    pageInfo {
      hasNextPage
      endCursor
    }
  }
}
```

**cURL GraphQL Request:**
```bash
curl -X POST http://localhost:5001/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "query { products(first: 10) { edges { node { id name price } } } }"
  }'
```

---

### 3. Shopping Cart Operations

**Add Item to Cart:**
```bash
curl -X POST http://localhost:5000/api/cart/items \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "prod-123",
    "quantity": 2,
    "price": 199.99
  }'
```

**Get Cart Items (GraphQL):**
```graphql
query {
  cart {
    id
    total
    itemCount
    items {
      id
      product {
        id
        name
        price
      }
      quantity
      subtotal
    }
  }
}
```

---

### 4. Order Placement

**Place Order:**
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "cartId": "cart-123",
    "shippingAddress": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    },
    "paymentMethod": "credit_card"
  }'
```

**Response:**
```json
{
  "orderId": "order-456",
  "status": "pending",
  "total": 399.98,
  "items": [
    {
      "productId": "prod-123",
      "quantity": 2,
      "price": 199.99
    }
  ],
  "createdAt": "2025-05-31T10:30:00Z"
}
```

---

### 5. Payment Processing

**Process Payment:**
```bash
curl -X POST http://localhost:5000/api/payments/process \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "order-456",
    "amount": 399.98,
    "currency": "USD",
    "paymentMethod": {
      "type": "credit_card",
      "cardNumber": "4111111111111111",
      "expiryMonth": 12,
      "expiryYear": 2026,
      "cvv": "123"
    }
  }'
```

**Response:**
```json
{
  "transactionId": "txn-789",
  "orderId": "order-456",
  "status": "completed",
  "amount": 399.98,
  "timestamp": "2025-05-31T10:35:00Z"
}
```

---

### 6. Event Monitoring

**Event Flow Example - Order Created:**

```
1. PlaceOrderCommand (YARP Gateway)
   ↓
2. OrderService processes command
   ↓
3. OrderCreatedEvent published to RabbitMQ
   ↓
4. Multiple subscribers receive event:
   - PaymentService: Initiates payment
   - EmailService: Sends confirmation email
   - InventoryService: Updates stock
   - NotificationService: Sends user notification
```

---

## ⚙️ Configuration

### Wolverine Configuration

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverine(options =>
{
    // Configure message routing
    options.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ"))
        .AutoProvision();

    // Configure message handlers
    options.HandlerGraph().RegisterAllHandlersFromNamespace(
        typeof(Program).Namespace);

    // Configure retry policies
    options.Policies.ConfigureConventionalFailureActions(
        x =>
        {
            x.ScheduleRetry<TransientException>(3.Seconds(), 10.Seconds());
            x.MoveToErrorQueue<PermanentException>();
        });
});
```

### YARP Configuration

**appsettings.json:**
```json
{
  "ReverseProxy": {
    "Routes": {
      "productsRoute": {
        "ClusterId": "productCluster",
        "Match": { "Path": "/api/products/{**catch-all}" },
        "Transforms": [
          { "RequestHeader": "X-Forwarded-For" },
          { "RequestHeader": "Authorization" }
        ]
      },
      "ordersRoute": {
        "ClusterId": "orderCluster",
        "Match": { "Path": "/api/orders/{**catch-all}" }
      }
    },
    "Clusters": {
      "productCluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://product-service:5002"
          }
        }
      }
    }
  }
}
```

### Security Configuration

**Two-Tier Validation Example:**

```csharp
// Tier 1: Gateway Level
public class AuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context, ITokenValidator validator)
    {
        var token = context.Request.Headers["Authorization"].ToString();
        
        if (!validator.IsValid(token))
            context.Response.StatusCode = 401;
        
        await _next(context);
    }
}

// Tier 2: Service Level
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderCommand cmd, ClaimsPrincipal user)
    {
        // Validate user has permission
        if (!user.HasClaim("order:create"))
            throw new UnauthorizedAccessException();
        
        // Validate business rules
        await ValidateOrderAsync(cmd);
        
        return await _repository.SaveAsync(cmd);
    }
}
```

---

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

### 1. Fork & Clone

```bash
git clone https://github.com/YOUR_USERNAME/E-Commerce_Microservice_SystemDesign_Templete.git
```

### 2. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Commit Changes

```bash
git commit -m "feat: add your feature description"
```

### 4. Push & Create Pull Request

```bash
git push origin feature/your-feature-name
```

### Code Standards

- Follow C# naming conventions (PascalCase for public, camelCase for private)
- Write unit tests for new features (xUnit)
- Maintain >80% code coverage
- Document complex business logic
- Follow SOLID principles
- Use async/await patterns throughout

### Project Structure

```
src/
├── Services/
│   ├── AuthService/
│   ├── ProductService/
│   ├── CartService/
│   ├── OrderService/
│   ├── PaymentService/
│   └── EmailService/
├── Gateways/
│   ├── YarpGateway/
│   └── GraphQLGateway/
├── Shared/
│   ├── Contracts/
│   ├── Events/
│   └── Utilities/
└── Tests/
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 Ahmed Hany

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

## 🆘 Support

### Getting Help

- **Documentation:** Check the [Docs](./docs) folder for detailed guides
- **Issues:** Report bugs or request features on [GitHub Issues](https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete/issues)
- **Discussions:** Join community discussions on [GitHub Discussions](https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete/discussions)

### Common Issues & Solutions

**RabbitMQ Connection Error:**
```bash
# Ensure RabbitMQ is running
docker ps | grep rabbitmq

# Check RabbitMQ logs
docker logs rabbitmq
```

**Database Migration Failed:**
```bash
# Reset database
dotnet ef database drop
dotnet ef database update
```

**Port Already in Use:**
```bash
# Find process on port 5000
lsof -i :5000
# Kill process
kill -9 <PID>
```

---

## 📞 Contact & Links

- **Author:** Ahmed Hany
- **Repository:** [GitHub](https://github.com/AhmedHany140/E-Commerce_Microservice_SystemDesign_Templete)
- **Email:** contact@ahmedhany.dev
- **LinkedIn:** [Ahmed Hany](https://linkedin.com/in/ahmedhany140)

---

## 🙏 Acknowledgments

Special thanks to the open-source community and the maintainers of:
- Wolverine Message Router
- YARP (Yet Another Reverse Proxy)
- HotChocolate GraphQL
- RabbitMQ
- .NET Foundation

---

**Last Updated:** May 31, 2025  
**Version:** 1.0.0  
**Status:** ✅ Production Ready
