using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddedStaticTeamOptionalFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_eventparticipantgroups_ScheduledEventId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups");

            migrationBuilder.AddColumn<Guid>(
                name: "EventParticipantGroupId1",
                schema: "sample_dot_net",
                table: "eventparticipants",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "team",
                schema: "sample_dot_net",
                columns: table => new
                {
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    SeasonStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SeasonEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_team_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "sample_dot_net",
                        principalTable: "tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staticteammembers",
                schema: "sample_dot_net",
                columns: table => new
                {
                    StaticTeamMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaticTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LeaveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staticteammembers", x => x.StaticTeamMemberId);
                    table.ForeignKey(
                        name: "FK_staticteammembers_players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "sample_dot_net",
                        principalTable: "players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staticteammembers_team_StaticTeamId",
                        column: x => x.StaticTeamId,
                        principalSchema: "sample_dot_net",
                        principalTable: "team",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipants_EventParticipantGroupId1",
                schema: "sample_dot_net",
                table: "eventparticipants",
                column: "EventParticipantGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipantgroups_ScheduledEventId_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                columns: new[] { "ScheduledEventId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipantgroups_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_staticteammembers_PlayerId",
                schema: "sample_dot_net",
                table: "staticteammembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_staticteammembers_StaticTeamId_PlayerId",
                schema: "sample_dot_net",
                table: "staticteammembers",
                columns: new[] { "StaticTeamId", "PlayerId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_team_TenantId",
                schema: "sample_dot_net",
                table: "team",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_eventparticipantgroups_team_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                column: "TeamId",
                principalSchema: "sample_dot_net",
                principalTable: "team",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_eventparticipants_eventparticipantgroups_EventParticipantG~1",
                schema: "sample_dot_net",
                table: "eventparticipants",
                column: "EventParticipantGroupId1",
                principalSchema: "sample_dot_net",
                principalTable: "eventparticipantgroups",
                principalColumn: "EventParticipantGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_eventparticipantgroups_team_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups");

            migrationBuilder.DropForeignKey(
                name: "FK_eventparticipants_eventparticipantgroups_EventParticipantG~1",
                schema: "sample_dot_net",
                table: "eventparticipants");

            migrationBuilder.DropTable(
                name: "staticteammembers",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "team",
                schema: "sample_dot_net");

            migrationBuilder.DropIndex(
                name: "IX_eventparticipants_EventParticipantGroupId1",
                schema: "sample_dot_net",
                table: "eventparticipants");

            migrationBuilder.DropIndex(
                name: "IX_eventparticipantgroups_ScheduledEventId_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups");

            migrationBuilder.DropIndex(
                name: "IX_eventparticipantgroups_TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups");

            migrationBuilder.DropColumn(
                name: "EventParticipantGroupId1",
                schema: "sample_dot_net",
                table: "eventparticipants");

            migrationBuilder.DropColumn(
                name: "TeamId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups");

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipantgroups_ScheduledEventId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                column: "ScheduledEventId");
        }
    }
}
