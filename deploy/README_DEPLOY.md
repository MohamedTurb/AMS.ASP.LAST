# Deployment Checklist (Production)

This file lists the minimal steps to deploy the Assistance Management System to a production server or container.

1. Prepare environment variables
   - Copy `.env.example` to `.env` and fill in real values (DB connection, secrets, SMTP, etc.)

2. Build backend
   - `dotnet publish backend -c Release -o ./publish`

3. Apply EF Core migrations
   - Ensure `ConnectionStrings__DefaultConnection` points to the production DB
   - Run: `dotnet ef database update --project backend --startup-project backend` or run migrations at startup

4. Seed roles and admin user
   - Ensure seed script runs on first start or run a one-off seeding command (see `database/` or `backend/Startup` seeder)

5. Build frontend and copy assets
   - Follow `frontend` build instructions (if using a bundler). Copy artifacts into `wwwroot/` or serve separately.

6. Start app (Docker example)
   - `docker compose -f docker-compose.yml up --build -d`
   - Or use the helper scripts in `deploy/`:

```bash
./deploy/start-prod.sh
```

```powershell
.\deploy\start-prod.ps1
```

7. Configure reverse proxy & TLS
   - Use Nginx, Traefik or cloud load balancer. Terminate TLS at the proxy.
   - Example Nginx config is provided at `deploy/nginx.conf`.
   - Use Certbot / Let's Encrypt to obtain certs and place them under `/etc/letsencrypt/live/<your-domain>/`.

8. Health checks & monitoring
   - Configure your orchestrator to hit `/health` or the app's health endpoints.
   - Centralize logs (Serilog sinks / AppInsights).

9. Backups and maintenance
   - Schedule DB backups and retention policy.

If you want, I can:
- Add startup migration/apply logic (call `dbContext.Database.Migrate()` on app start).
- Create a seed script to create roles and an admin user.
- Harden the `Dockerfile` and `docker-compose.yml` for production.

CI/CD
-- A sample GitHub Actions workflow is added at `.github/workflows/ci.yml` to build and test the project.
-- To push docker images, set `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` in the repository secrets.
