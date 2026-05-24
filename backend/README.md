# Backend

This folder groups the server-side application logic of the current ASP.NET Core 8 project.

Current mapped components:
- `Program.cs` -> app startup, DI, authentication, middleware
- `controllers/` -> MVC and API controllers
- `services/` -> business services and helper services
- `contracts/` -> API DTOs and request/response models
- `models/` -> domain models and Identity user model
- `data/` -> `ApplicationDbContext` and database initializer
- `migrations/` -> EF Core migrations and model snapshot
- `config/` -> environment configuration (`appsettings*.json`)
- `AssistanceManagementSystem.csproj` -> project file in the repo root

Suggested substructure:
- `controllers/`
- `contracts/`
- `data/`
- `models/`
- `services/`
- `migrations/`
- `config/`

Note: The live source files remain in the root project folders until a full repo restructure is explicitly requested.