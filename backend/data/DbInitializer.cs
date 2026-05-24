using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // In CI/test environments where migrations may not have been generated,
            // prefer EnsureCreated to build schema from the current model.
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
            if (env.Equals("Testing", StringComparison.OrdinalIgnoreCase))
            {
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                // Apply migrations to keep database schema in sync with EF model.
                await context.Database.MigrateAsync();
            }

            // Ensure RefreshTokens table exists (fallback for test environments).
            try
            {
                var conn = context.Database.GetDbConnection();
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS RefreshTokens (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        TokenHash TEXT NOT NULL,
                                        UserId TEXT NOT NULL,
                                        Expires TEXT NOT NULL,
                                        IsRevoked INTEGER NOT NULL,
                                        CreatedAt TEXT NOT NULL,
                                        ReplacedByToken TEXT
                                    );
                                    CREATE UNIQUE INDEX IF NOT EXISTS IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);";
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // ignore - best-effort fallback for tests
            }

            // Create roles
            string[] roles = { "Admin", "Branch Manager", "Staff", "Approver", "Beneficiary" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create branches
            if (!context.Branches.Any())
            {
                var branchNames = new[]
                {
                    "القاهرة - القاهرة",
                    "القاهرة - المقطم",
                    "القاهرة - الزيتون",
                    "القاهرة - الحى السابع",
                    "القاهرة - اسماء فهمى",
                    "القاهرة - النصر",
                    "القاهرة - حلوان",
                    "القاهرة - عين حلوان",
                    "الاسكندريه - احمد عرابى",
                    "الاسكندريه - السلطان حسين",
                    "الشرقيه - الزقازيق",
                    "الشرقيه - مشتول السوق",
                    "الشرقيه - ميت العز",
                    "الشرقيه - اولاد سيف",
                    "الشرقيه - الجوسق- بلبيس",
                    "الشرقيه - الزوامل",
                    "الشرقيه - الديدامون",
                    "اسيوط - اسيوط",
                    "اسيوط - بنى رافع",
                    "اسيوط - الحواتكه",
                    "الغربيه - طنطا",
                    "الغربيه - الفاتح",
                    "الغربيه - كفر الشرفا",
                    "الغربيه - كفر الشيخ سليم",
                    "الغربيه - نشيل",
                    "الغربيه - بنوفر",
                    "الغربيه - شبرابيل",
                    "المنوفيه - شبين الكوم",
                    "المنوفيه - السادات",
                    "المنوفيه - منشاه عصام",
                    "المنوفيه - الباجور",
                    "المنوفيه - الخطاطبه",
                    "الفيوم - الفيوم",
                    "بنى سويف - بنى سويف",
                    "بورسعيد - بورسعيد",
                    "السويس - السويس",
                    "شمال سيناء - العريش",
                    "جنوب سيناء - الطور",
                    "الاسماعيليه - الاسماعيليه",
                    "الاسماعيليه - التل الكبير",
                    "الوادى الجديد - الوادى الجديد",
                    "الوادى الجديد - الداخله",
                    "البحيره - دمنهور",
                    "البحيره - بدر",
                    "قنا - قنا",
                    "قنا - قوص",
                    "المنيا - المنيا",
                    "المنيا - بنى محمد سلطان",
                    "المنيا - المغالقه",
                    "المنيا - مغاغه",
                    "المنيا - دير مواس",
                    "المنيا - ابو عزيز",
                    "اسوان - اسوان",
                    "اسوان - ادفو",
                    "اسوان - البصيليه",
                    "اسوان - سلوا بحرى",
                    "اسوان - كوم امبو",
                    "الدقهليه - المنصوره",
                    "الدقهليه - برج النور",
                    "الدقهليه - دماص",
                    "الدقهليه - ديو الوسطى",
                    "الدقهليه - ابو داود السباخ",
                    "الدقهليه - ميت تمامه",
                    "الدقهليه - نجير",
                    "الدقهليه - كفر علام",
                    "الدقهليه - بلقاس",
                    "المحله الكبرى - المحله الكبرى",
                    "المحله الكبرى - المعتمديه",
                    "المحله الكبرى - محله زياد",
                    "المحله الكبرى - ميت عساس",
                    "المحله الكبرى - بشبيش",
                    "المحله الكبرى - السجاعيه",
                    "المحله الكبرى - ميت هاشم",
                    "المحله الكبرى - صفط تراب",
                    "سوهاج - سوهاج",
                    "سوهاج - شندويل",
                    "سوهاج - جزيره شندويل",
                    "سوهاج - طهطا",
                    "دمياط - دمياط",
                    "دمياط - الرحامنه",
                    "دمياط - الزرقا",
                    "القليوبيه - بنها",
                    "القليوبيه - شبرا الخيمه",
                    "كفر الشيخ - كفر الشيخ",
                    "كفر الشيخ - الخادميه",
                    "الجيزه - الجيزة",
                    "الجيزه - المهندسين",
                    "الجيزه - اكتوبر",
                    "الاقصر - الاقصر",
                    "الاقصر - المحاميد",
                    "البحر الاحمر - سفاجا",
                    "البحر الاحمر - شلاتين"
                };

                var branches = branchNames.Select(n => new Branch { Name = n, Address = string.Empty, Phone = string.Empty }).ToList();
                context.Branches.AddRange(branches);
                await context.SaveChangesAsync();
            }

            // Create aid categories
            if (!context.AidCategories.Any())
            {
                var categories = new List<AidCategory>();

                AidCategory Add(string nameAr, string code, string scope = "Both", int sortOrder = 0, bool allowCustomText = false, string? parentCode = null)
                {
                    var category = new AidCategory
                    {
                        NameAr = nameAr,
                        Code = code,
                        Scope = scope,
                        SortOrder = sortOrder,
                        AllowCustomText = allowCustomText
                    };

                    if (!string.IsNullOrWhiteSpace(parentCode))
                    {
                        category.ParentAidCategory = categories.First(c => c.Code == parentCode);
                    }

                    categories.Add(category);
                    return category;
                }

                Add("مساعدات مرضية", "MED", allowCustomText: true);
                Add("فشل كلوى", "MED-RENAL", allowCustomText: true, parentCode: "MED");
                Add("أورام", "MED-TUMOR", allowCustomText: true, parentCode: "MED");
                Add("قلب", "MED-HEART", allowCustomText: true, parentCode: "MED");
                Add("أنيميا بحر متوسط", "MED-THAL", allowCustomText: true, parentCode: "MED");
                Add("متلازمة داون", "MED-DS", allowCustomText: true, parentCode: "MED");
                Add("تصلب متناثر", "MED-MS", allowCustomText: true, parentCode: "MED");
                Add("ألزهايمر", "MED-ALZ", allowCustomText: true, parentCode: "MED");
                Add("نقل دم", "MED-BLOOD", allowCustomText: true, parentCode: "MED");
                Add("علاج", "MED-TREAT", allowCustomText: true, parentCode: "MED");
                Add("أشعات", "MED-IMAGING", parentCode: "MED");
                Add("تحاليل", "MED-LABS", parentCode: "MED");

                Add("عمليات جراحية", "SURG", allowCustomText: true);
                Add("زراعة كبد", "SURG-LIVER", allowCustomText: true, parentCode: "SURG");
                Add("زراعة قلب", "SURG-HEART", allowCustomText: true, parentCode: "SURG");
                Add("زراعة كلى", "SURG-KIDNEY", allowCustomText: true, parentCode: "SURG");
                Add("زراعة نخاع", "SURG-BM", allowCustomText: true, parentCode: "SURG");
                Add("زراعة قوقعة", "SURG-COCHLEA", allowCustomText: true, parentCode: "SURG");

                Add("حالات اجتماعية", "SOC", allowCustomText: true);
                Add("أسرة مسجون", "SOC-PRISON", parentCode: "SOC");
                Add("مطلقة", "SOC-DIVORCED", parentCode: "SOC");
                Add("مهجورة", "SOC-ABANDONED", parentCode: "SOC");
                Add("أيتام", "SOC-ORPHAN", parentCode: "SOC");
                Add("كريمي النسب", "SOC-UNKNOWN", parentCode: "SOC");
                Add("أسر بديلة", "SOC-FOSTER", parentCode: "SOC");
                Add("مساعدات اجتماعية حسب النوع", "SOC-GENDER", parentCode: "SOC");
                Add("مساعدات اجتماعية حسب السن", "SOC-AGE", parentCode: "SOC");

                Add("إعاقات وأجهزة", "DIS", allowCustomText: true);
                Add("إعاقات ذهنية", "DIS-MENTAL", parentCode: "DIS");
                Add("إعاقات سمعية", "DIS-HEARING", parentCode: "DIS");
                Add("إعاقات بصرية", "DIS-VISION", parentCode: "DIS");
                Add("إعاقات متعددة", "DIS-MULTI", parentCode: "DIS");
                Add("سماعات أذن", "DIS-HEARINGAID", parentCode: "DIS");
                Add("أطراف صناعية", "DIS-PROSTHESIS", parentCode: "DIS");
                Add("أجهزة تعويضية", "DIS-ASSISTIVE", parentCode: "DIS");
                Add("أجهزة شق حنجري", "DIS-TRACH", parentCode: "DIS");
                Add("أجهزة طبية للأفراد", "DIS-MED-IND", parentCode: "DIS");
                Add("أجهزة طبية للجهات", "DIS-MED-ORG", parentCode: "DIS");
                Add("مستلزمات طبية أفراد", "DIS-SUP-IND", parentCode: "DIS");
                Add("مستلزمات طبية جهات", "DIS-SUP-ORG", parentCode: "DIS");
                Add("عربات بخارية عادية", "DIS-SCOOTER", parentCode: "DIS");
                Add("عربات بخارية كهربائية", "DIS-SCOOTER-E", parentCode: "DIS");
                Add("كراسى متحركة عادية", "DIS-WHEEL", parentCode: "DIS");
                Add("كراسى متحركة كهربائية", "DIS-WHEEL-E", parentCode: "DIS");
                Add("تغيير أجهزة قوقعة", "DIS-COCHLEA-REPL", parentCode: "DIS");
                Add("صيانة أجهزة قوقعة", "DIS-COCHLEA-MAINT", parentCode: "DIS");

                Add("تعليم", "EDU", allowCustomText: true);
                Add("مصروفات جامعية", "EDU-UNI", parentCode: "EDU");
                Add("مصروفات دراسية", "EDU-SCHOOL", parentCode: "EDU");
                Add("محو أمية", "EDU-LIT", parentCode: "EDU");

                Add("كوارث وحرائق", "EMR", allowCustomText: true);
                Add("كوارث", "EMR-DISASTER", parentCode: "EMR");
                Add("حرائق", "EMR-FIRE", parentCode: "EMR");

                Add("خدمات ومشاريع", "PRJ", allowCustomText: true);
                Add("توصيل مياه شرب", "PRJ-WATER", parentCode: "PRJ");
                Add("تجهيز وحدات", "PRJ-UNIT", parentCode: "PRJ");
                Add("شراء أسهم وحدات", "PRJ-SHARES", parentCode: "PRJ");
                Add("مشروعات أفراد", "PRJ-IND", parentCode: "PRJ");
                Add("مشروعات جهات", "PRJ-ORG", parentCode: "PRJ");
                Add("مشروعات قومية", "PRJ-NATIONAL", parentCode: "PRJ");

                Add("أخرى", "GEN-OTHER", allowCustomText: true);

                context.AidCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Create users
            if (!context.Users.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@ams.com",
                    Email = "admin@ams.com",
                    FullName = "مدير النظام",
                    BranchId = 1,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                var managerUser = new ApplicationUser
                {
                    UserName = "manager@ams.com",
                    Email = "manager@ams.com",
                    FullName = "مدير الفرع",
                    BranchId = 1,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(managerUser, "Manager123!");
                await userManager.AddToRoleAsync(managerUser, "Branch Manager");

                var staffUser = new ApplicationUser
                {
                    UserName = "staff@ams.com",
                    Email = "staff@ams.com",
                    FullName = "موظف",
                    BranchId = 1,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(staffUser, "Staff123!");
                await userManager.AddToRoleAsync(staffUser, "Staff");

                var approverUser = new ApplicationUser
                {
                    UserName = "approver@ams.com",
                    Email = "approver@ams.com",
                    FullName = "معتمد",
                    BranchId = 1,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(approverUser, "Approver123!");
                await userManager.AddToRoleAsync(approverUser, "Approver");

                var beneficiaryUser = new ApplicationUser
                {
                    UserName = "beneficiary@ams.com",
                    Email = "beneficiary@ams.com",
                    FullName = "مستفيد",
                    BranchId = 1,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(beneficiaryUser, "Beneficiary123!");
                await userManager.AddToRoleAsync(beneficiaryUser, "Beneficiary");

                // Create one manager and one staff account per branch
                var allBranches = context.Branches.ToList();
                foreach (var branch in allBranches)
                {
                    var mgrEmail = $"manager{branch.Id}@ams.com";
                    if (await userManager.FindByEmailAsync(mgrEmail) == null)
                    {
                        var mgr = new ApplicationUser
                        {
                            UserName = mgrEmail,
                            Email = mgrEmail,
                            FullName = $"مدير {branch.Name}",
                            BranchId = branch.Id,
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(mgr, "Manager123!");
                        await userManager.AddToRoleAsync(mgr, "Branch Manager");
                    }

                    var staffEmail = $"staff{branch.Id}@ams.com";
                    if (await userManager.FindByEmailAsync(staffEmail) == null)
                    {
                        var st = new ApplicationUser
                        {
                            UserName = staffEmail,
                            Email = staffEmail,
                            FullName = $"موظف {branch.Name}",
                            BranchId = branch.Id,
                            EmailConfirmed = true
                        };
                        await userManager.CreateAsync(st, "Staff123!");
                        await userManager.AddToRoleAsync(st, "Staff");
                    }
                }
            }

            // Create beneficiaries
            if (!context.Beneficiaries.Any())
            {
                var beneficiaries = new List<Beneficiary>
                {
                    new Beneficiary { FullName = "أحمد محمد علي", NationalId = "12345678901234", Phone = "01012345678", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "متزوج", FamilyMembers = 4, Income = 3000, BranchId = 1 },
                    new Beneficiary { FullName = "فاطمة أحمد حسن", NationalId = "23456789012345", Phone = "01023456789", Gender = "أنثى", Religion = "مسلمة", MaritalStatus = "أرملة", FamilyMembers = 3, Income = 1500, BranchId = 1 },
                    new Beneficiary { FullName = "محمد سعد إبراهيم", NationalId = "34567890123456", Phone = "01034567890", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "أعزب", FamilyMembers = 1, Income = 2000, BranchId = 2 },
                    new Beneficiary { FullName = "نور الدين محمود", NationalId = "45678901234567", Phone = "01045678901", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "متزوج", FamilyMembers = 5, Income = 4000, BranchId = 2 },
                    new Beneficiary { FullName = "مريم عبد الرحمن", NationalId = "56789012345678", Phone = "01056789012", Gender = "أنثى", Religion = "مسلمة", MaritalStatus = "مطلقة", FamilyMembers = 2, Income = 1200, BranchId = 3 },
                    new Beneficiary { FullName = "علي حسن محمد", NationalId = "67890123456789", Phone = "01067890123", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "متزوج", FamilyMembers = 6, Income = 2500, BranchId = 3 },
                    new Beneficiary { FullName = "سارة أحمد فؤاد", NationalId = "78901234567890", Phone = "01078901234", Gender = "أنثى", Religion = "مسلمة", MaritalStatus = "متزوجة", FamilyMembers = 4, Income = 1800, BranchId = 4 },
                    new Beneficiary { FullName = "خالد محمود سعد", NationalId = "89012345678901", Phone = "01089012345", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "أعزب", FamilyMembers = 1, Income = 2200, BranchId = 4 },
                    new Beneficiary { FullName = "هند محمد علي", NationalId = "90123456789012", Phone = "01090123456", Gender = "أنثى", Religion = "مسلمة", MaritalStatus = "أرملة", FamilyMembers = 3, Income = 1000, BranchId = 5 },
                    new Beneficiary { FullName = "يوسف أحمد حسن", NationalId = "01234567890123", Phone = "01001234567", Gender = "ذكر", Religion = "مسلم", MaritalStatus = "متزوج", FamilyMembers = 5, Income = 3500, BranchId = 5 }
                };
                context.Beneficiaries.AddRange(beneficiaries);
                await context.SaveChangesAsync();
            }

            // Create organizations
            if (!context.Organizations.Any())
            {
                var organizations = new List<Organization>
                {
                    new Organization { Name = "جمعية البر والإحسان", Type = "جمعية خيرية", Address = "شارع التحرير، القاهرة", Phone = "02-12345678", AccountNumber = "12345678901234567890" },
                    new Organization { Name = "مؤسسة الأمل للتنمية", Type = "مؤسسة تنموية", Address = "شارع الهرم، الجيزة", Phone = "02-87654321", AccountNumber = "23456789012345678901" },
                    new Organization { Name = "جمعية الرحمة الخيرية", Type = "جمعية خيرية", Address = "شارع سعد زغلول، الإسكندرية", Phone = "03-11111111", AccountNumber = "34567890123456789012" },
                    new Organization { Name = "مؤسسة النور للخدمات", Type = "مؤسسة خدمات", Address = "شارع الجمهورية، المنيا", Phone = "086-2222222", AccountNumber = "45678901234567890123" },
                    new Organization { Name = "جمعية الخير والبركة", Type = "جمعية خيرية", Address = "شارع جامعة أسيوط، أسيوط", Phone = "088-3333333", AccountNumber = "56789012345678901234" }
                };
                context.Organizations.AddRange(organizations);
                await context.SaveChangesAsync();
            }

            // Create projects
            if (!context.Projects.Any())
            {
                var projects = new List<Project>
                {
                    new Project { Name = "مشروع كفالة الأيتام", Type = "كفالة", Address = "مقر الجمعية الرئيسي", Phone = "02-12345678" },
                    new Project { Name = "مشروع المساعدات الطارئة", Type = "مساعدات", Address = "مقر الجمعية الرئيسي", Phone = "02-12345678" },
                    new Project { Name = "مشروع التعليم والتدريب", Type = "تعليم", Address = "مركز التدريب", Phone = "02-87654321" }
                };
                context.Projects.AddRange(projects);
                await context.SaveChangesAsync();
            }

            // Create assistances
            if (!context.Assistances.Any())
            {
                var assistances = new List<Assistance>
                {
                    new Assistance { BeneficiaryId = 1, Type = "مساعدات مالية", Amount = 500, PaymentMethod = "نقدي", Status = "Approved", Notes = "مساعدة شهرية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 2, Type = "مساعدات غذائية", Amount = 200, PaymentMethod = "عيني", Status = "Paid", Notes = "سلة غذائية شهرية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 3, Type = "مساعدات طبية", Amount = 1000, PaymentMethod = "تحويل بنكي", Status = "Pending", Notes = "تكاليف عملية جراحية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 4, Type = "مساعدات تعليمية", Amount = 300, PaymentMethod = "نقدي", Status = "Approved", Notes = "رسوم مدرسية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 5, Type = "مساعدات سكنية", Amount = 800, PaymentMethod = "تحويل بنكي", Status = "Rejected", Notes = "إيجار منزل", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 6, Type = "مساعدات مالية", Amount = 400, PaymentMethod = "نقدي", Status = "Paid", Notes = "مساعدة طارئة", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 7, Type = "مساعدات غذائية", Amount = 150, PaymentMethod = "عيني", Status = "Approved", Notes = "سلة غذائية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 8, Type = "مساعدات طبية", Amount = 600, PaymentMethod = "تحويل بنكي", Status = "Pending", Notes = "فحوصات طبية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 9, Type = "مساعدات تعليمية", Amount = 250, PaymentMethod = "نقدي", Status = "Approved", Notes = "كتب مدرسية", CreatedByUserId = context.Users.First().Id },
                    new Assistance { BeneficiaryId = 10, Type = "مساعدات مالية", Amount = 700, PaymentMethod = "تحويل بنكي", Status = "Paid", Notes = "مساعدة شهرية", CreatedByUserId = context.Users.First().Id }
                };
                context.Assistances.AddRange(assistances);
                await context.SaveChangesAsync();
            }
        }
    }
}
