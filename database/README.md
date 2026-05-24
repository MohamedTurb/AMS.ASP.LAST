# Database

This folder groups database-related assets for the project.

Current mapped components:
- `add_assistance_request_table.sql` -> manual SQL script
- `migrations/` -> EF Core migration history
- `backend/data/DbInitializer.cs` -> automatic seeding and schema initialization
- `AssistanceManagementSystem.db` -> local SQLite database file

Suggested substructure:
- `scripts/` -> raw SQL scripts
- `migrations/` -> exported or reference migration scripts
- `seeds/` -> seed data scripts and fixtures

The file `scripts/add_assistance_request_table.sql` is a copy of the root SQL script for easier browsing by database-focused work.