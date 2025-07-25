using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OluBackendApp.Migrations
{
    /// <inheritdoc />
    public partial class Update_JobPostDecimalAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_OfficeOwnerProfiles_Id",
                table: "OfficeOwnerProfiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "JobPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OfficeOwnerProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPosts_OfficeOwnerProfiles_OfficeOwnerProfileId",
                        column: x => x.OfficeOwnerProfileId,
                        principalTable: "OfficeOwnerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_OfficeOwnerProfileId",
                table: "JobPosts",
                column: "OfficeOwnerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPosts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_OfficeOwnerProfiles_Id",
                table: "OfficeOwnerProfiles");
        }
    }
}
