using AssistanceManagementSystem.Data;
using AssistanceManagementSystem.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace AssistanceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Branch Manager")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Branches = await _context.Branches.ToListAsync();
            ViewBag.Types = await _context.Assistances.Select(a => a.Type).Distinct().ToListAsync();
            ViewBag.Statuses = new List<string> { "Pending", "Approved", "Paid", "Rejected" };
            
            return View();
        }

        public async Task<IActionResult> Analytics(int? branchFilter, DateTime? dateFrom, DateTime? dateTo)
        {
            ViewBag.Branches = await _context.Branches.OrderBy(b => b.Name).ToListAsync();

            var requestsQuery = _context.AssistanceRequests
                .Include(a => a.Branch)
                .AsQueryable();

            if (branchFilter.HasValue)
            {
                requestsQuery = requestsQuery.Where(a => a.BranchId == branchFilter.Value);
            }

            if (dateFrom.HasValue)
            {
                requestsQuery = requestsQuery.Where(a => a.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                requestsQuery = requestsQuery.Where(a => a.CreatedAt <= dateTo.Value);
            }

            var requests = await requestsQuery.ToListAsync();
            var totalAmount = requests.Where(a => a.Amount.HasValue).Sum(a => a.Amount!.Value);
            var averageAmount = requests.Where(a => a.Amount.HasValue).Select(a => a.Amount!.Value).DefaultIfEmpty().Average();

            var statusOrder = new[] { "معلق", "موافق عليه", "مرفوض", "تم التنفيذ" };
            var statusCounts = statusOrder
                .Select(status => new StatusCountItem(status, requests.Count(a => a.Status == status)))
                .ToList();

            var typeCounts = requests
                .GroupBy(a => string.IsNullOrWhiteSpace(a.TypeOfAssistance) ? "غير محدد" : a.TypeOfAssistance)
                .Select(g => new LabelCountItem(g.Key, g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(8)
                .ToList();

            var branchCounts = requests
                .GroupBy(a => a.Branch?.Name ?? "بدون فرع")
                .Select(g => new LabelCountItem(g.Key, g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(8)
                .ToList();

            var monthLabels = Enumerable.Range(0, 6)
                .Select(offset => DateTime.Today.AddMonths(-5 + offset))
                .Select(date => new DateTime(date.Year, date.Month, 1))
                .ToList();

            var monthlyCounts = monthLabels
                .Select(month => requests.Count(a => a.CreatedAt.Year == month.Year && a.CreatedAt.Month == month.Month))
                .ToList();

            var viewModel = new AnalyticsViewModel
            {
                TotalRequests = requests.Count,
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                PendingRequests = statusCounts.First(x => x.Label == "معلق").Count,
                ApprovedRequests = statusCounts.First(x => x.Label == "موافق عليه").Count,
                RejectedRequests = statusCounts.First(x => x.Label == "مرفوض").Count,
                ExecutedRequests = statusCounts.First(x => x.Label == "تم التنفيذ").Count,
                StatusLabels = statusCounts.Select(x => x.Label).ToList(),
                StatusCounts = statusCounts.Select(x => x.Count).ToList(),
                TypeLabels = typeCounts.Select(x => x.Label).ToList(),
                TypeCounts = typeCounts.Select(x => x.Count).ToList(),
                BranchLabels = branchCounts.Select(x => x.Label).ToList(),
                BranchCounts = branchCounts.Select(x => x.Count).ToList(),
                MonthLabels = monthLabels.Select(m => m.ToString("yyyy/MM")).ToList(),
                MonthCounts = monthlyCounts,
                TopType = typeCounts.FirstOrDefault()?.Label ?? "غير متوفر",
                TopBranch = branchCounts.FirstOrDefault()?.Label ?? "غير متوفر"
            };

            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
            ViewBag.BranchFilter = branchFilter;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportType, string typeFilter, string statusFilter, int? branchFilter, DateTime? dateFrom, DateTime? dateTo)
        {
            var assistances = _context.Assistances
                .Include(a => a.Beneficiary)
                .Include(a => a.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(typeFilter))
                assistances = assistances.Where(a => a.Type == typeFilter);

            if (!string.IsNullOrEmpty(statusFilter))
                assistances = assistances.Where(a => a.Status == statusFilter);

            if (branchFilter.HasValue)
                assistances = assistances.Where(a => a.Beneficiary.BranchId == branchFilter);

            if (dateFrom.HasValue)
                assistances = assistances.Where(a => a.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                assistances = assistances.Where(a => a.CreatedAt <= dateTo.Value);

            var data = await assistances.ToListAsync();
            var branchName = string.Empty;
            if (branchFilter.HasValue)
            {
                var branch = await _context.Branches.FindAsync(branchFilter.Value);
                branchName = branch?.Name ?? string.Empty;
            }

            return reportType.ToLower() switch
            {
                "excel" => await ExportToExcel(data, branchName),
                "pdf" => await ExportToPdf(data, branchName),
                _ => BadRequest("نوع التقرير غير صحيح")
            };
        }

        private async Task<IActionResult> ExportToExcel(List<Assistance> assistances, string branchName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("تقرير المساعدات");

            // Headers
            worksheet.Cell(1, 1).Value = "رقم المساعدة";
            worksheet.Cell(1, 2).Value = "اسم المستفيد";
            worksheet.Cell(1, 3).Value = "الفرع";
            worksheet.Cell(1, 4).Value = "نوع المساعدة";
            worksheet.Cell(1, 5).Value = "المبلغ (جنيه)";
            worksheet.Cell(1, 6).Value = "طريقة الدفع";
            worksheet.Cell(1, 7).Value = "الحالة";
            worksheet.Cell(1, 8).Value = "الملاحظات";
            worksheet.Cell(1, 9).Value = "تاريخ الإنشاء";

            // Data
            for (int i = 0; i < assistances.Count; i++)
            {
                var assistance = assistances[i];
                var row = i + 2;
                
                worksheet.Cell(row, 1).Value = assistance.Id;
                worksheet.Cell(row, 2).Value = assistance.Beneficiary.FullName;
                worksheet.Cell(row, 3).Value = assistance.Beneficiary?.Branch?.Name;
                worksheet.Cell(row, 4).Value = assistance.Type;
                worksheet.Cell(row, 5).Value = assistance.Amount;
                worksheet.Cell(row, 6).Value = assistance.PaymentMethod;
                worksheet.Cell(row, 7).Value = assistance.Status;
                worksheet.Cell(row, 8).Value = assistance.Notes;
                worksheet.Cell(row, 9).Value = assistance.CreatedAt.ToString("yyyy-MM-dd");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = string.IsNullOrEmpty(branchName)
                ? $"تقرير_المساعدات_{DateTime.Now:yyyyMMdd}.xlsx"
                : $"تقرير_المساعدات_{branchName}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<IActionResult> ExportToPdf(List<Assistance> assistances, string branchName)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("تقرير المساعدات")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                     columns.RelativeColumn();
                                     columns.RelativeColumn();
                                     columns.RelativeColumn();
                                     columns.RelativeColumn();
                                     columns.RelativeColumn();
                                     columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("رقم المساعدة").FontSize(10).Bold();
                                     header.Cell().Text("رقم المساعدة").FontSize(10).Bold();
                                     header.Cell().Text("اسم المستفيد").FontSize(10).Bold();
                                     header.Cell().Text("الفرع").FontSize(10).Bold();
                                     header.Cell().Text("نوع المساعدة").FontSize(10).Bold();
                                     header.Cell().Text("المبلغ").FontSize(10).Bold();
                                     header.Cell().Text("الحالة").FontSize(10).Bold();
                                });

                                foreach (var assistance in assistances)
                                {
                                     table.Cell().Text(assistance.Id.ToString());
                                     table.Cell().Text(assistance.Beneficiary?.FullName ?? string.Empty);
                                     table.Cell().Text(assistance.Beneficiary?.Branch?.Name ?? "");
                                     table.Cell().Text(assistance.Type);
                                     table.Cell().Text($"{assistance.Amount:N0} جنيه");
                                     table.Cell().Text(assistance.Status);
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("صفحة ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();
            var fileName = string.IsNullOrEmpty(branchName)
                ? $"تقرير_المساعدات_{DateTime.Now:yyyyMMdd}.pdf"
                : $"تقرير_المساعدات_{branchName}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }

    public class AnalyticsViewModel
    {
        public int TotalRequests { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int ExecutedRequests { get; set; }
        public string TopType { get; set; } = string.Empty;
        public string TopBranch { get; set; } = string.Empty;
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
        public List<string> TypeLabels { get; set; } = new();
        public List<int> TypeCounts { get; set; } = new();
        public List<string> BranchLabels { get; set; } = new();
        public List<int> BranchCounts { get; set; } = new();
        public List<string> MonthLabels { get; set; } = new();
        public List<int> MonthCounts { get; set; } = new();
    }

    public sealed record StatusCountItem(string Label, int Count);
    public sealed record LabelCountItem(string Label, int Count);
}
