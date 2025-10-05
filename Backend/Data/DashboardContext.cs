using DashboardApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardApi.Data;

public class DashboardContext : DbContext
{
    public DashboardContext(DbContextOptions<DashboardContext> options) : base(options)
    {
    }

    public DbSet<DashboardItem> DashboardItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<DashboardItem>().HasData(
            new DashboardItem { Id = 1, Title = "Sales Revenue", Description = "Q1 2024 Revenue", Value = 125000.50m, Category = "Revenue", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DashboardItem { Id = 2, Title = "Active Users", Description = "Monthly Active Users", Value = 15420, Category = "Users", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DashboardItem { Id = 3, Title = "Conversion Rate", Description = "Current Conversion Rate", Value = 3.75m, Category = "Metrics", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DashboardItem { Id = 4, Title = "Customer Satisfaction", Description = "CSAT Score", Value = 4.6m, Category = "Satisfaction", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new DashboardItem { Id = 5, Title = "Orders Processed", Description = "Total Orders This Month", Value = 8945, Category = "Orders", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
