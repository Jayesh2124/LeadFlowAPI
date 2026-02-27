# LeadFlow Backend Architecture Guide

Welcome to the **LeadFlow API** backend documentation. This guide is designed to help you understand the core architecture, the design patterns used, and how to start contributing to the project effectively.

---

## 1. Architectural Overview: Clean Architecture

The LeadFlow API is built following **Clean Architecture** (also known as Onion Architecture). The primary goal of this architecture is **Separation of Concerns**.

### The Layers
The project is divided into four main projects (layers) within the `src` folder:

1.  **LeadFlow.Domain (The Core)**
    *   **Purpose**: Contains pure enterprise logic and business objects (Entities).
    *   **Rules**: It has **zero dependencies** on other layers or external libraries (like EF Core).
    *   **Contents**: Entities (`User`, `Lead`, `EmailTask`), Enums, Domain Events, and Custom Exceptions.

2.  **LeadFlow.Application (Business Logic)**
    *   **Purpose**: Orchestrates the flow of data and defines "what" the system does.
    *   **Rules**: Depends only on the Domain layer. Uses Interfaces to talk to external services.
    *   **Contents**: Features (CQRS Handlers), Validators (FluentValidation), Models (DTOs), and Service Interfaces.

3.  **LeadFlow.Infrastructure (External Services)**
    *   **Purpose**: Implementation of external concerns. "How" we save data or send emails.
    *   **Rules**: Depends on Domain and Application.
    *   **Contents**: `AppDbContext` (EF Core), JWT Token generation, Hangfire Job implementations, and SMTP client logic.

4.  **LeadFlow.API (The Entry Point)**
    *   **Purpose**: The public face of the system (REST API).
    *   **Rules**: Depends on Application and Infrastructure (to wire them up).
    *   **Contents**: Controllers, Middleware, and API configuration (`Program.cs`).

---

## 2. Key Design Patterns

### CQRS with MediatR
We use **CQRS** (Command Query Responsibility Segregation) to split our logic:
-   **Commands**: Operations that change state (Create, Update, Delete).
-   **Queries**: Operations that only read data.

**MediatR** acts as a "postman". Instead of a Controller calling a Service directly, it sends a "Request" (Command/Query) to MediatR, which finds the correct "Handler" to execute it.

### Why MediatR?
-   **Decoupling**: Controllers don't need to know which service handles the logic.
-   **Lean Controllers**: Controllers only have 1-2 lines of code.
-   **Easier Testing**: You can test a single Handler in isolation.

### Domain-Driven Design (DDD) Lite
We use static factory methods (e.g., `User.Create(...)`) instead of public constructors. This ensures that an object is always in a "valid state" when created.

---

## 3. Technology Stack

-   **.NET 8**: The latest cross-platform framework.
-   **Entity Framework Core**: Our ORM (Object-Relational Mapper) for database access.
-   **Hangfire**: Used for background processing (e.g., sending scheduled emails).
-   **FluentValidation**: For consistent and readable input validation.
-   **BCrypt.Net**: For secure password hashing.

---

## 4. The Core Engine: Email Automation

The heart of LeadFlow is the automated email processing system, located in `LeadFlow.Infrastructure/BackgroundJobs/HangfireEmailTaskProcessor.cs`.

### How it Works:
1.  **Scheduling**: When a lead is assigned a template, an `EmailTask` is created with a `ScheduledSendDate`.
2.  **Hangfire Integration**: We use Hangfire to enqueue a job. Hangfire handles the timing and ensures the task runs even if the server restarts.
3.  **Process Flow**:
    *   **Fetch**: Load the task and associated lead/SMTP settings.
    *   **Render**: Replace placeholders like `{{FirstName}}` with actual lead data.
    *   **Send**: Use the user's specific SMTP settings to send the email.
    *   **Retry**: If it fails, it uses **Exponential Backoff** (1h, 2h, 4h...) to try again.
    *   **Follow-up**: Upon success, it checks if there are follow-up templates scheduled and enqueues them automatically.

---

## 5. Implementation Plan: How to Add a New Feature

If you are a beginner assigned to add a new feature (e.g., "Manage Tags"), follow these steps:

### Step 1: Define the Entity (Domain)
Add a new class in `LeadFlow.Domain/Entities/Tag.cs`. Inherit from `BaseEntity`.
```csharp
public class Tag : BaseEntity
{
    public string Name { get; private set; }
    // Private constructor for EF
    private Tag() { } 
    // Factory method for domain logic
    public static Tag Create(string name) => new Tag { Name = name };
}
```

### Step 2: Add to DbContext (Infrastructure)
Register the new entity in `LeadFlow.Infrastructure/Persistence/AppDbContext.cs`.

### Step 3: Create the Command/Query (Application)
Navigate to `LeadFlow.Application/Features`. Create a folder for your feature.
1.  **Command**: A record defining the input (e.g., `CreateTagCommand`).
2.  **Validator**: Create a `CreateTagValidator : AbstractValidator<CreateTagCommand>`.
3.  **Handler**: Create a class implementing `IRequestHandler<CreateTagCommand, Result<Guid>>`.

### Step 4: Create the API Endpoint (API)
Add a new method in a relevant class within `LeadFlow.API/Endpoints/` that sends the command via MediatR:
```csharp
app.MapPost("/tags", async (IMediator mediator, CreateTagCommand command) =>
{
    var result = await mediator.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});
```

---

## 6. Pro-Tips for Beginners

### 1. Use the `Result<T>` Pattern
Instead of throwing exceptions for things that are expected (like "User not found"), return `Result.Failure("Message")`. This makes the code predictable and easier to debug.

### 2. Primary Constructors
We use C# 12 **Primary Constructors**. You'll see classes defined like `public class MyHandler(IDbContext db)`. This automatically creates a private field for `db`.

### 3. AsNoTracking()
When writing **Queries** (read-only), always use `.AsNoTracking()` in your LINQ queries. It improves performance by telling EF Core not to track changes to those objects.

### 4. Thin Endpoints, Fat Handlers
Keep your Endpoints empty. All "if" statements and business logic should stay inside the `Handle` method of your Application features.

---

### Useful Folders to Explore
-   `src/LeadFlow.Application/Common`: Contains base classes like `Result.cs`.
-   `src/LeadFlow.Application/Features`: This is where 90% of your work will happen.
-   `src/LeadFlow.Infrastructure/BackgroundJobs`: The engine that powers the automation.
-   `src/LeadFlow.Domain/Entities`: The blueprint of our data.
