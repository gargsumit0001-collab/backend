# Payments Portal

## Backend
- .NET 8 Web API with JWT + refresh tokens
- BCrypt password hashing
- Repository pattern, UnitOfWork, PaymentService, DemoPaymentGateway
- SQLite (payments.db created automatically)

Run backend:
```bash
cd backend/PaymentApi
dotnet restore
dotnet run
```

Swagger: http://localhost:7000/swagger

Seeded credentials (on first run):
- username: admin
- password: Admin@123

## Frontend
- Angular 17 app with login page, JWT interceptor, auth guard, payments forms

Run frontend:
cd frontend
npm install
npm start
```

