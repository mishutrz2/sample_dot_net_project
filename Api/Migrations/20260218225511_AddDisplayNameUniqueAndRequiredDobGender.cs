using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameUniqueAndRequiredDobGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                schema: "sample_dot_net",
                table: "appusers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "sample_dot_net",
                table: "appusers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_appusers_DisplayName",
                schema: "sample_dot_net",
                table: "appusers",
                column: "DisplayName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appusers_DisplayName",
                schema: "sample_dot_net",
                table: "appusers");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "sample_dot_net",
                table: "appusers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                schema: "sample_dot_net",
                table: "appusers",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
