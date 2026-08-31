# Loan Deduction Prediction System with Behavioral Analytics

## 1. Project Overview

The **Loan Deduction Prediction System with Behavioral Analytics** is a web-based application developed to manage loans and monitor the repayment behavior of borrowers.

The system helps manage the complete loan repayment process starting from loan creation to repayment tracking and risk prediction.

The application allows authorized users to:

- Manage users.
- Create and manage loans.
- Calculate EMI.
- Generate repayment schedules.
- Record loan payments.
- Track borrower payment behavior.
- Generate risk predictions.
- Generate alerts.
- View dashboards.
- Control access based on user roles.

The main purpose of the project is to provide a centralized system where loan information and repayment behavior can be managed in an organized way.

The system uses **ASP.NET Core Web API** for the backend, **Entity Framework Core** for database communication, and **Microsoft SQL Server** for storing application data.

---

# 2. Problem Statement

Traditional loan management systems mainly focus on storing loan details and repayment information.

However, simply storing loan information is not enough to understand whether a borrower may face repayment difficulties.

For example:

- A borrower may delay payments.
- A borrower may make partial payments.
- A borrower may have a high outstanding loan balance.
- A borrower may repeatedly miss repayment dates.

If this information is not properly tracked, it becomes difficult for loan officers to identify repayment risks.

Therefore, this project provides a system that combines:

- Loan management
- Repayment schedule management
- Payment tracking
- Payment behavior logging
- Risk prediction
- Alert generation

This helps loan officers and administrators understand the repayment status of borrowers more effectively.

---

# 3. Main Objective

The main objective of this project is to develop a secure loan management system that can track borrower repayment behavior and provide risk-related information.

The system is designed to:

1. Manage users and their roles.
2. Manage borrower loan accounts.
3. Calculate monthly EMI.
4. Generate repayment schedules.
5. Record payments.
6. Track payment behavior.
7. Generate risk predictions.
8. Provide alerts.
9. Provide role-based access.
10. Protect sensitive user information.

---

# 4. Users of the System

The system has three main user roles:

### 4.1 Administrator

The Administrator has high-level access to the system.

The Administrator can:

- View loan information.
- Manage loan-related operations.
- View repayment information.
- View risk information.
- View alerts.
- Access the administrator dashboard.

---

### 4.2 Loan Officer

The Loan Officer manages loans assigned to them.

The Loan Officer can:

- View assigned loans.
- Create loans assigned to themselves.
- Generate repayment schedules.
- Record payments.
- View repayment information.
- View risk information.
- View alerts.
- Access the loan officer dashboard.

A Loan Officer cannot access loans assigned to another Loan Officer.

---

### 4.3 Borrower

The Borrower is the customer who has taken the loan.

The Borrower can:

- View their own loans.
- View their repayment schedules.
- View their repayment information.
- View their alerts.

A Borrower cannot access another borrower's loan information.

---

# 5. Main Features

The major features implemented in the system are:

### Authentication

- Login using email and password.
- Password verification.
- BCrypt password hashing.
- JWT token generation.
- Refresh token support.

### Authorization

- Role-based authorization.
- Ownership-based authorization.
- Admin access.
- Loan Officer access.
- Borrower access.

### Loan Management

- Create loan.
- View loan.
- View all loans.
- View borrower's loans.
- View loan officer's loans.
- Update loan status.

### EMI Calculation

- Calculate monthly EMI.
- Store EMI amount with the loan.

### Repayment Schedule

- Generate repayment schedule.
- View repayment schedule.
- View individual installment.
- View repayment summary.

### Payment Management

- Record payment.
- Update paid amount.
- Store payment date.
- Update payment status.

### Payment Behavior

- Record payment behavior.
- Calculate days late.
- Store payment status.

### Risk Prediction

- Generate risk score.
- Generate risk level.
- Store prediction reason.
- Store prediction date.

### Alerts

- Retrieve alerts.
- Retrieve alerts for a particular loan.
- Provide relevant loan-related notifications.

### Dashboard

- Admin dashboard.
- Loan Officer dashboard.

---

# 6. Technology Stack

The project uses the following technologies.

## Backend

- C#
- ASP.NET Core Web API
- .NET

## Database

- Microsoft SQL Server

## ORM

- Entity Framework Core

## Authentication

- JWT Bearer Authentication
- BCrypt Password Hashing
- Refresh Tokens

## API Documentation

- Swagger
- OpenAPI

## Development Tools

- Visual Studio
- Visual Studio Code
- PowerShell
- SQLCMD

---

# 7. System Architecture

The project follows a layered architecture.

The basic architecture is:

Client
↓
API Controller
↓
Service Layer
↓
Repository Layer
↓
Entity Framework Core
↓
SQL Server

---

## 7.1 API Layer

The API layer contains the controllers.

Controllers receive requests from the client and send responses back to the client.

Examples of controllers include:

- Authentication Controller
- Loan Controller
- Repayment Schedule Controller
- Alert Controller
- Dashboard Controller

The API layer is responsible for:

- Receiving HTTP requests.
- Validating requests.
- Checking authorization.
- Calling services.
- Returning HTTP responses.

---

## 7.2 Service Layer

The Service Layer contains the business logic.

The controller does not directly communicate with the database.

Instead:

Controller
↓
Service
↓
Repository

The service layer performs operations such as:

- Loan creation.
- EMI calculation.
- Repayment schedule generation.
- Payment processing.
- Payment behavior processing.
- Risk prediction.
- Alert processing.
- Dashboard calculations.

---

## 7.3 Repository Layer

The Repository Layer communicates with the database through Entity Framework Core.

Repositories perform operations such as:

- SELECT
- INSERT
- UPDATE
- DELETE

Examples include:

- UserRepository
- LoanRepository
- RepaymentScheduleRepository
- PaymentBehaviorRepository
- RiskPredictionRepository
- AlertRepository

---

## 7.4 Database Layer

The database layer uses Microsoft SQL Server.

Entity Framework Core is used to communicate between the application and SQL Server.

The database contains the main tables required for:

- Users
- Loans
- Repayment schedules
- Payment behavior
- Risk predictions
- Refresh tokens

---

# 8. Why Layered Architecture Is Used

The layered architecture provides separation of responsibilities.

For example:

The Controller handles HTTP requests.

The Service handles business logic.

The Repository handles database operations.

This makes the project:

- Easier to understand.
- Easier to maintain.
- Easier to test.
- Easier to modify.
- Easier to extend.

---

# 9. Authentication

The system uses JWT-based authentication.

The login process works as follows:

User
↓
Enter Email and Password
↓
API receives login request
↓
Password is verified
↓
JWT token is generated
↓
User uses token for protected APIs

The token is sent using:

Authorization: Bearer <JWT_TOKEN>

---

# 10. Password Security

Passwords are not stored as plain text.

The project uses **BCrypt** for password hashing.

For example:

User enters:

```text

###

Migration:

dotnet ef migrations add AddPayment --project LoanDeductionPrediction.Repositories --startup-project LoanDeductionPrediction.API

SQl DB:

dotnet ef database update --project LoanDeductionPrediction.Repositories --startup-project LoanDeductionPrediction.API

