# نظام إدارة المساعدات (AMS)

تطبيق ويب لإدارة طلبات المساعدة، المنظمات، الفروع، والمشاريع. المشروع مبني باستخدام ASP.NET Core (Backend) وواجهة Frontend داخل المجلد `frontend/` ويشمل قواعد بيانات، سكربتات نشر، واختبارات وحدة وتكامل.

**المسار العام للمشروع**
- `backend/` — مصدر التطبيق (Controllers, Models, Services, Data, Migrations)
- `frontend/` — صفحات الواجهة، assets، مكونات وملفات CSS/JS
- `database/` — سكربتات، هجرات مخصصة، وملفات seeds
- `AssistanceManagementSystem.Tests/` — اختبارات وحدة وتكامل وE2E

**التقنيات المستخدمة**
- .NET 8, ASP.NET Core MVC / Web API
- Entity Framework Core 8 (Code First)
- ASP.NET Core Identity (Roles & Users)
- SQLite (تطوير) وSQL Server (إنتاج)
- Swagger / OpenAPI, Serilog, Health Checks
- ClosedXML (تصدير Excel), QuestPDF (تصدير PDF)

**ملخّص ما تمّ تنفيذه بالفعل**
- REST API كاملة مع versioning (v1)
- Authentication: JWT + Refresh Tokens + External OAuth (Google, Microsoft)
- إدارة المستخدمين والأدوار (Admin, Branch Manager, Staff, Approver, Beneficiary)
- CRUD كامل للمنظمات، المشاريع، الفروع، التصنيفات، وطلبات المساعدة
- رفع ملفات ودعم التحقق من الامتدادات والحجم
- تقارير Excel وPDF (Export)
- ترحيل تلقائي للـ EF عند التشغيل (`Migrate()`)
- Seeding لبيانات مبدئية (أدوار وحسابات تجريبية)
- Logging عبر Serilog وملفات rolling داخل `logs/`
- صفحات وإدارة للتصنيفات تحت `frontend/pages/AidCategories/`
- اختبارات: Integration tests, E2E login flow, Unit tests في المشروع `AssistanceManagementSystem.Tests`
- ملفات ونماذج جاهزة لـ Docker (`Dockerfile`, `docker-compose.yml`)
- ملفات تشغيل مساعدة: `run.bat`, `run.ps1`, `start-app.bat`, `start-app.ps1`

**قائمة الكنترولرز الأساسية (جزء مما تمّ تنفيذه)**
- `AccountController` — تسجيل/تسجيل دخول/تسجيل خارجي
- `AssistanceRequestsController` — إدارة طلبات المساعدة
- `AssistancesController`
- `AidCategoriesController` — إدارة التصنيفات (CRUD + صفحات Razor)
- `BeneficiariesController`, `BeneficiariesApiController`
- `BranchesController`, `BranchesApiController`
- `OrganizationsController`
- `ProjectsController`
- `NotificationsController`
- `ReportsController`

## التشغيل محليًا (خُطّة سريعة)
1. ثبت متطلبات التعريف:

```powershell
dotnet --list-sdks
```

2. استعادة الحزم وبناء المشروع:

```powershell
dotnet restore
dotnet build
```

3. تشغيل من جذر المشروع (يستخدم الإعدادات من `appsettings.Development.json`):

```powershell
#$Env:ASPNETCORE_ENVIRONMENT='Development'
#$Env:AMS_PORT='5002' # أو 5003 إذا ضبطت المنفذ
dotnet run --project AssistanceManagementSystem.csproj
```

أو تشغيل المجلد `backend` مباشرة:

```powershell
dotnet run --project backend
```

ملاحظة: تستخدم بعض الملفات النصية `run.ps1` و`start-app.ps1` لتسهيل الإعداد. إذا ظهر خطأ بمنفذ، تأكد أن المنفذ المطلوب غير مستخدم أو عدّله في المتغير `AMS_PORT` أو `launchSettings.json`.

## متغيرات بيئة مهمة
- `ASPNETCORE_ENVIRONMENT` (Development/Production)
- `AMS_PORT` (المنفذ المخصص)
- `ConnectionStrings__SqlServerConnection` (سلسلة الاتصال للإنتاج)
- `Jwt__SecretKey` (مفتاح توقيع JWT)
- OAuth secrets: `Authentication__Google__ClientId`, ...

## تشغيل الاختبارات

```powershell
dotnet test AssistanceManagementSystem.Tests/AssistanceManagementSystem.Tests.csproj
```

تتضمن الاختبارات وحدات، اختبارات تكامل تتحقق من `/health` و`/api/v1/auth/token`، وE2E لتدفق تسجيل الدخول.

## Docker

لبناء وتشغيل بالحاويات:

```powershell
docker compose up --build
```

ثم افتح المتصفح على `http://localhost:5002` أو المنفذ الذي حددته.

## نشر (نصائح عامة)
- تأكد من ضبط `ConnectionStrings` والإعدادات السرية في بيئة الإنتاج.
- شغّل `dotnet ef database update` على بيئة الإنتاج أو استخدم آلية CI/CD لتنفيذ الترحيلات قبل نشر الكود.
- احتفظ بنسخة احتياطية من قاعدة البيانات قبل تطبيق الترحيلات الكبيرة.

## مراقبة وتشغيل
- Health endpoint: `/health`
- سجّل الأخطاء والعمليات عبر Serilog داخل `logs/`.

## حسابات تجريبية (غير مخصصة للإنتاج)

| الدور | البريد الإلكتروني | كلمة المرور |
|------:|------------------:|------------:|
| Admin | admin@ams.com | Admin123! |
| Branch Manager | manager@ams.com | Manager123! |
| Staff | staff@ams.com | Staff123! |
| Approver | approver@ams.com | Approver123! |
| Beneficiary | beneficiary@ams.com | Beneficiary123! |

## ملاحظات معروفة ومواضيع للمراجعة
- قد تحتاج تحديثات في واجهة `frontend` لتوافق تغييرات API بعد الترحيلات.
- تحقق من سياسات رفع الملفات (حجم/نوع) حسب بيئة الإنتاج.

## أين أجد الكود المهم
- [Controllers](backend/controllers/) — منطق الـ HTTP
- [Data / ApplicationDbContext.cs](backend/data/ApplicationDbContext.cs) — إعداد EF
- [Migrations](backend/migrations/) — ملفات EF
- [frontend/pages/](frontend/pages/) — صفحات الواجهة
- [AssistanceManagementSystem.Tests/](AssistanceManagementSystem.Tests/) — الاختبارات

---

قمت بتحديث هذا الملف ليشمل كل المكونات والميزات المطبقة حاليًا. راجع المحتوى وقل لي إن أردت إضافة تفاصيل تقنية أعمق، خرائط ER للقاعدة، أو أمثلة API كاملة لكل endpoint.
