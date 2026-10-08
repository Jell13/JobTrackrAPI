using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTrackrAPI.Migrations
{
    /// <inheritdoc />
    public partial class addEmployerSponsorshipStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employer_sponsorship_stats",
                columns: table => new
                {
                    employer_key = table.Column<string>(type: "text", nullable: false),
                    employer_name = table.Column<string>(type: "text", nullable: false),
                    total_filings = table.Column<int>(type: "integer", nullable: false),
                    certified_filings = table.Column<int>(type: "integer", nullable: false),
                    certification_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    typical_wage_from = table.Column<decimal>(type: "numeric", nullable: true),
                    typical_wage_to = table.Column<decimal>(type: "numeric", nullable: true),
                    top_job_title = table.Column<string>(type: "text", nullable: true),
                    most_recent_decision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employer_sponsorship_stats", x => x.employer_key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employer_sponsorship_stats");
        }
    }
}
