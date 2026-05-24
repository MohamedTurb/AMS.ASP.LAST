using AssistanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssistanceManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Assistance> Assistances { get; set; }
        public DbSet<AssistanceRequest> AssistanceRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AidCategory> AidCategories { get; set; }
        public DbSet<ReviewAudit> ReviewAudits { get; set; }
        public DbSet<BeneficiaryTransfer> BeneficiaryTransfers { get; set; }
        public DbSet<BranchSettings> BranchSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure unique constraint for NationalId
            builder.Entity<Beneficiary>()
                .HasIndex(b => b.NationalId)
                .IsUnique();

            // Configure relationships
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Beneficiary>()
                .HasOne(b => b.Branch)
                .WithMany(br => br.Beneficiaries)
                .HasForeignKey(b => b.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Beneficiary>()
                .HasOne(b => b.AidCategory)
                .WithMany()
                .HasForeignKey(b => b.AidCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Assistance>()
                .HasOne(a => a.Beneficiary)
                .WithMany(b => b.Assistances)
                .HasForeignKey(a => a.BeneficiaryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Assistance>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Link Assistance -> AssistanceRequest (optional)
            builder.Entity<Assistance>()
                .HasOne(a => a.AssistanceRequest)
                .WithMany(ar => ar.Assistances)
                .HasForeignKey(a => a.AssistanceRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure unique constraint for RequesterNationalId in AssistanceRequest
            builder.Entity<AssistanceRequest>()
                .HasIndex(ar => ar.RequesterNationalId)
                .IsUnique();

            // Configure relationships for AssistanceRequest
            builder.Entity<AssistanceRequest>()
                .HasOne(ar => ar.CreatedByUser)
                .WithMany()
                .HasForeignKey(ar => ar.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AssistanceRequest>()
                .HasOne(ar => ar.AidCategory)
                .WithMany()
                .HasForeignKey(ar => ar.AidCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Link AssistanceRequest -> Branch (optional)
            builder.Entity<AssistanceRequest>()
                .HasOne(ar => ar.Branch)
                .WithMany()
                .HasForeignKey(ar => ar.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AidCategory>()
                .HasOne(a => a.ParentAidCategory)
                .WithMany(a => a.Children)
                .HasForeignKey(a => a.ParentAidCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // RefreshToken configuration - index on hashed token
            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            builder.Entity<AssistanceRequest>()
                .HasOne(ar => ar.ReviewedByUser)
                .WithMany()
                .HasForeignKey(ar => ar.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReviewAudit
            builder.Entity<ReviewAudit>()
                .HasOne(ra => ra.AssistanceRequest)
                .WithMany(ar => ar.ReviewAudits)
                .HasForeignKey(ra => ra.AssistanceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // BeneficiaryTransfer relations
            builder.Entity<BeneficiaryTransfer>()
                .HasOne(bt => bt.Beneficiary)
                .WithMany()
                .HasForeignKey(bt => bt.BeneficiaryId)
                .OnDelete(DeleteBehavior.Cascade);

            // BranchSettings
            builder.Entity<BranchSettings>()
                .HasOne(bs => bs.Branch)
                .WithMany()
                .HasForeignKey(bs => bs.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project -> Branch
            builder.Entity<Project>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.Projects)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            // Organization -> Branch
            builder.Entity<Organization>()
                .HasOne(o => o.Branch)
                .WithMany(b => b.Organizations)
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            // Notification
            builder.Entity<Notification>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
