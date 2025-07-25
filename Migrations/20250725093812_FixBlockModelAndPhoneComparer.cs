using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OluBackendApp.Migrations
{
    /// <inheritdoc />
    public partial class FixBlockModelAndPhoneComparer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateOfResidence",
                table: "ArtisanProfiles");

            migrationBuilder.RenameColumn(
                name: "StateOfOrigin",
                table: "ArtisanProfiles",
                newName: "State");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                table: "ArtisanProfiles",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutYou",
                table: "ArtisanProfiles",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                table: "ArtisanProfiles",
                newName: "StateOfOrigin");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "ArtisanProfiles",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AboutYou",
                table: "ArtisanProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateOfResidence",
                table: "ArtisanProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
