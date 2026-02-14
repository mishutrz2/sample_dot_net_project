using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedVersionTypeForEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE sample_dot_net.""ScheduledEvents"" 
                DROP COLUMN ""Version"";
            ");

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "sample_dot_net",
                table: "ScheduledEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE sample_dot_net.""ScheduledEvents"" 
                DROP COLUMN ""Version"";
            ");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "sample_dot_net",
                table: "ScheduledEvents",
                type: "bytea",
                nullable: false);
        }
    }
}
