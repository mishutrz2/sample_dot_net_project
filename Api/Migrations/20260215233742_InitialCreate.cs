using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sample_dot_net");

            migrationBuilder.CreateTable(
                name: "activities",
                schema: "sample_dot_net",
                columns: table => new
                {
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.ActivityId);
                });

            migrationBuilder.CreateTable(
                name: "appusers",
                schema: "sample_dot_net",
                columns: table => new
                {
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwsSubject = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appusers", x => x.AppUserId);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "sample_dot_net",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "sample_dot_net",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "rolepermissions",
                schema: "sample_dot_net",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolepermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_rolepermissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "sample_dot_net",
                        principalTable: "permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rolepermissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "sample_dot_net",
                        principalTable: "roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "sample_dot_net",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_tenants_activities_ActivityId",
                        column: x => x.ActivityId,
                        principalSchema: "sample_dot_net",
                        principalTable: "activities",
                        principalColumn: "ActivityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenants_roles_DefaultRoleId",
                        column: x => x.DefaultRoleId,
                        principalSchema: "sample_dot_net",
                        principalTable: "roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memberships",
                schema: "sample_dot_net",
                columns: table => new
                {
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memberships", x => new { x.AppUserId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_memberships_appusers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "sample_dot_net",
                        principalTable: "appusers",
                        principalColumn: "AppUserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_memberships_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "sample_dot_net",
                        principalTable: "roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memberships_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "sample_dot_net",
                        principalTable: "tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "sample_dot_net",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_players_appusers_AppUserId",
                        column: x => x.AppUserId,
                        principalSchema: "sample_dot_net",
                        principalTable: "appusers",
                        principalColumn: "AppUserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_players_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "sample_dot_net",
                        principalTable: "tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduledevents",
                schema: "sample_dot_net",
                columns: table => new
                {
                    ScheduledEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsProjected = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduledevents", x => x.ScheduledEventId);
                    table.ForeignKey(
                        name: "FK_scheduledevents_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "sample_dot_net",
                        principalTable: "tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventparticipantgroups",
                schema: "sample_dot_net",
                columns: table => new
                {
                    EventParticipantGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventparticipantgroups", x => x.EventParticipantGroupId);
                    table.ForeignKey(
                        name: "FK_eventparticipantgroups_scheduledevents_ScheduledEventId",
                        column: x => x.ScheduledEventId,
                        principalSchema: "sample_dot_net",
                        principalTable: "scheduledevents",
                        principalColumn: "ScheduledEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventparticipants",
                schema: "sample_dot_net",
                columns: table => new
                {
                    EventParticipantGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventparticipants", x => new { x.EventParticipantGroupId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_eventparticipants_eventparticipantgroups_EventParticipantGr~",
                        column: x => x.EventParticipantGroupId,
                        principalSchema: "sample_dot_net",
                        principalTable: "eventparticipantgroups",
                        principalColumn: "EventParticipantGroupId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventparticipants_players_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "sample_dot_net",
                        principalTable: "players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appusers_AwsSubject",
                schema: "sample_dot_net",
                table: "appusers",
                column: "AwsSubject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipantgroups_ScheduledEventId",
                schema: "sample_dot_net",
                table: "eventparticipantgroups",
                column: "ScheduledEventId");

            migrationBuilder.CreateIndex(
                name: "IX_eventparticipants_PlayerId",
                schema: "sample_dot_net",
                table: "eventparticipants",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_AppUserId_TenantId",
                schema: "sample_dot_net",
                table: "memberships",
                columns: new[] { "AppUserId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_memberships_RoleId",
                schema: "sample_dot_net",
                table: "memberships",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_TenantId",
                schema: "sample_dot_net",
                table: "memberships",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_players_AppUserId",
                schema: "sample_dot_net",
                table: "players",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_players_TenantId_AppUserId",
                schema: "sample_dot_net",
                table: "players",
                columns: new[] { "TenantId", "AppUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rolepermissions_PermissionId",
                schema: "sample_dot_net",
                table: "rolepermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_scheduledevents_TenantId",
                schema: "sample_dot_net",
                table: "scheduledevents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_ActivityId",
                schema: "sample_dot_net",
                table: "tenants",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_DefaultRoleId",
                schema: "sample_dot_net",
                table: "tenants",
                column: "DefaultRoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventparticipants",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "memberships",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "rolepermissions",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "eventparticipantgroups",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "players",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "scheduledevents",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "appusers",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "activities",
                schema: "sample_dot_net");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "sample_dot_net");
        }
    }
}
