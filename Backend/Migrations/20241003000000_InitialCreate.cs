using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DashboardApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardItems", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DashboardItems",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Title", "Value" },
                values: new object[,]
                {
                    { 1, "Revenue", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Q1 2024 Revenue", "Sales Revenue", 125000.50m },
                    { 2, "Users", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monthly Active Users", "Active Users", 15420m },
                    { 3, "Metrics", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Current Conversion Rate", "Conversion Rate", 3.75m },
                    { 4, "Satisfaction", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CSAT Score", "Customer Satisfaction", 4.6m },
                    { 5, "Orders", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Total Orders This Month", "Orders Processed", 8945m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardItems");
        }
    }
}
