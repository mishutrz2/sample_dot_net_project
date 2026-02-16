using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedEventResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventresults",
                schema: "sample_dot_net",
                columns: table => new
                {
                    EventResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    WinningGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ResultData = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventresults", x => x.EventResultId);
                    table.ForeignKey(
                        name: "FK_eventresults_eventparticipantgroups_WinningGroupId",
                        column: x => x.WinningGroupId,
                        principalSchema: "sample_dot_net",
                        principalTable: "eventparticipantgroups",
                        principalColumn: "EventParticipantGroupId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_eventresults_scheduledevents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalSchema: "sample_dot_net",
                        principalTable: "scheduledevents",
                        principalColumn: "ScheduledEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventresults_ScheduledEventId",
                schema: "sample_dot_net",
                table: "eventresults",
                column: "ScheduledEventId");

            migrationBuilder.CreateIndex(
                name: "IX_eventresults_WinningGroupId",
                schema: "sample_dot_net",
                table: "eventresults",
                column: "WinningGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventresults",
                schema: "sample_dot_net");
        }
    }
}
