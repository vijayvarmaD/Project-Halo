using Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppRole>(b =>
            {
                b.ToTable("AspNetRoles");
            });

            modelBuilder.Entity<Employee>(emp =>
            {
                emp.HasMany(e => e.DirectReportees).WithOne(e => e.EmployeeReportsTo).HasForeignKey(e => e.ReportsTo).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PolicyRequirement>().HasKey(pr => new { pr.PolicyId, pr.RequirementId });
            modelBuilder.Entity<PolicyRequirement>().HasOne(pr => pr.Policy).WithMany(r => r.PolicyRequirements).HasForeignKey(pr => pr.PolicyId);
            modelBuilder.Entity<PolicyRequirement>().HasOne(pr => pr.Requirement).WithMany(r => r.PolicyRequirements).HasForeignKey(pr => pr.RequirementId);

        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmployeeDesignation> EmployeeDesignation { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Requirement> Requirements { get; set; }

        public DbSet<Policy> Policies { get; set; }
    }
}
