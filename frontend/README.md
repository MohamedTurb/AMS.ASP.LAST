# Frontend

This folder groups the user-facing presentation layer of the current ASP.NET Core MVC app.

Current mapped components:
- `pages/` -> Razor pages, shared layouts, `_ViewImports`, `_ViewStart`
- `assets/` -> static assets, uploads, CSS, JS, images

Suggested substructure:
- `components/` -> reusable UI pieces
- `pages/` -> page-level views
- `assets/` -> images, icons, fonts
- `styles/` -> custom CSS
- `scripts/` -> custom JavaScript

Note: The MVC runtime is configured to load views from `frontend/pages` and static files from `frontend/assets`.