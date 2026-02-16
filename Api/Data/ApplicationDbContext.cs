using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ScheduledEvent> ScheduledEvents => Set<ScheduledEvent>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<EventParticipantGroup> EventParticipantGroups => Set<EventParticipantGroup>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<EventResult> EventResults => Set<EventResult>();
    public DbSet<StaticTeam> StaticTeams => Set<StaticTeam>();
    public DbSet<StaticTeamMember> StaticTeamMembers => Set<StaticTeamMember>();

    private void ApplySoftDeleteLogic()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
        }
    }

    public override int SaveChanges()
    {
        ApplySoftDeleteLogic();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDeleteLogic();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("sample_dot_net");

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()!.ToLower());
        }

        // Primary key column naming: Id -> {EntityName}Id
        builder.Entity<AppUser>().Property(u => u.Id).HasColumnName("AppUserId");
        builder.Entity<Activity>().Property(a => a.Id).HasColumnName("ActivityId");
        builder.Entity<Tenant>().Property(t => t.Id).HasColumnName("TenantId");
        builder.Entity<ScheduledEvent>().Property(e => e.Id).HasColumnName("ScheduledEventId");
        builder.Entity<Player>().Property(p => p.Id).HasColumnName("PlayerId");
        builder.Entity<Role>().Property(r => r.Id).HasColumnName("RoleId");
        builder.Entity<Permission>().Property(p => p.Id).HasColumnName("PermissionId");
        builder.Entity<EventParticipantGroup>().Property(epg => epg.Id).HasColumnName("EventParticipantGroupId");
        builder.Entity<EventResult>().Property(er => er.Id).HasColumnName("EventResultId");
        builder.Entity<StaticTeam>().Property(st => st.Id).HasColumnName("TeamId");
        builder.Entity<StaticTeamMember>().Property(stm => stm.Id).HasColumnName("StaticTeamMemberId");
        
        builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Activity>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<ScheduledEvent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Player>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<Permission>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<EventParticipantGroup>().HasQueryFilter(epg => !epg.IsDeleted);
        builder.Entity<EventResult>().HasQueryFilter(er => !er.IsDeleted);
        builder.Entity<Membership>().HasQueryFilter(m => !m.AppUser.IsDeleted);

        builder.Entity<Team>().Property(t => t.TeamType).HasConversion<string>();

        // Identity indexes
        builder.Entity<AppUser>().HasIndex(u => u.AwsSubject).IsUnique();

        // Useful indexes
        builder.Entity<ScheduledEvent>().HasIndex(e => e.TenantId);
        builder.Entity<Player>().HasIndex(p => new { p.TenantId, p.AppUserId }).IsUnique();
        builder.Entity<Membership>().HasIndex(m => new { m.AppUserId, m.TenantId });
        builder.Entity<EventParticipantGroup>().HasIndex(epg => new { epg.ScheduledEventId, epg.TeamId });

        // ========================================
        // MANY-TO-MANY RELATIONSHIPS
        // ========================================

        // MEMBERSHIP: AppUser <-> Tenant (with Role payload)
        // Purpose: Track which users belong to which leagues/tenants
        // Example: User "John" is "Admin" in "Football League A" and "Player" in "Tennis League B"
        // Composite key prevents duplicate memberships
        // Cascades deletion: deleting a user or tenant removes all memberships
        builder.Entity<Membership>()
            .HasKey(m => new { m.AppUserId, m.TenantId });

        builder.Entity<Membership>()
            .HasOne(m => m.AppUser)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Membership>()
            .HasOne(m => m.Tenant)
            .WithMany(t => t.Memberships)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Role assignment from Membership
        // Cascade Restrict: Prevents deleting a role if it's assigned to active memberships
        builder.Entity<Membership>()
            .HasOne(m => m.Role)
            .WithMany(r => r.Memberships)
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // ROLE PERMISSIONS: Role <-> Permission
        // Purpose: Define what each role can do (RBAC)
        // Example: "Admin" role can create events, manage players, etc.
        // Composite key prevents duplicate role-permission assignments
        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // EVENT PARTICIPANTS: Player -> EventParticipantGroup (composite M2M)
        // Composite key prevents same player from being assigned to same group twice
        builder.Entity<EventParticipant>()
            .HasKey(ep => new { ep.EventParticipantGroupId, ep.PlayerId });

        // ========================================
        // ONE-TO-MANY RELATIONSHIPS
        // ========================================

        // ACTIVITY -> TENANT: Different sport/competition types per league
        // Purpose: Classify what competition type this tenant hosts
        // Example: "Football" activity → multiple soccer leagues
        // Restrict: Can't delete activity if tenants depend on it
        builder.Entity<Tenant>()
            .HasOne(t => t.Activity)
            .WithMany(a => a.Tenants)
            .HasForeignKey(t => t.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        // DEFAULT ROLE: Each tenant has a default role for new members
        // Purpose: Automatically assign new joiners a role
        // Example: New users joining a league automatically get "Player" role
        // Restrict: Can't delete a role if it's a tenant's default
        builder.Entity<Tenant>()
            .HasOne(t => t.DefaultRole)
            .WithMany()
            .HasForeignKey(t => t.DefaultRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // SCHEDULED EVENTS: Tenant hosts multiple events/matches
        // Purpose: Track all events in a league
        // Example: "Football League A" hosts weekly matches
        // Cascade: Deleting tenant deletes all its events
        builder.Entity<ScheduledEvent>()
            .HasOne(e => e.Tenant)
            .WithMany(t => t.ScheduledEvents)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // PLAYERS: Tenant has multiple player records
        // Purpose: Represents a user's identity within a specific tenant
        // Example: User "John" has a "Player" record in "Football League A"
        // Allows different display names, stats per league
        // Unique constraint: One player record per (tenant, appuser) pair
        // Cascade: Deleting tenant removes all player records
        builder.Entity<Player>()
            .HasOne(p => p.Tenant)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // AppUser -> Multiple Player records across tenants
        // Purpose: Enable user to be a player in multiple leagues
        // Example: "John" has Player records in [Football League A, Tennis League B, Chess Club]
        builder.Entity<Player>()
            .HasOne(p => p.AppUser)
            .WithMany()
            .HasForeignKey(p => p.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // EVENT PARTICIPANT GROUPS: Team formations within an event
        // Purpose: Define teams/groups for a match
        // Example: Event "Football Match A" might have:
        //   - Group 1: "Team A" (players: John, Jane, Jack)
        //   - Group 2: "Team B" (players: Alice, Annie, Andrew)
        // For tennis 1v1, each group has 1 player
        // For volleyball 6v6, each group has 6 players
        // Cascade: Deleting event deletes all group formations
        builder.Entity<EventParticipantGroup>()
            .HasOne(epg => epg.ScheduledEvent)
            .WithMany()
            .HasForeignKey(epg => epg.ScheduledEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // EVENT PARTICIPANTS: Individual player assignments to event groups
        // Purpose: Track which players are in which team/group for an event
        // Example: Event "Football Match A", Group 1 "Team A" contains:
        //   - Participant 1: John
        //   - Participant 2: Jane
        //   - Participant 3: Jack
        // Composite key: (EventParticipantGroupId, PlayerId) prevents duplicates
        builder.Entity<EventParticipant>()
            .HasOne(ep => ep.EventParticipantGroup)
            .WithMany()
            .HasForeignKey(ep => ep.EventParticipantGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Player reference in EventParticipant
        // Links to the actual player participating
        builder.Entity<EventParticipant>()
            .HasOne(ep => ep.Player)
            .WithMany()
            .HasForeignKey(ep => ep.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // EVENT RESULT: Stores outcome of a ScheduledEvent
        // Purpose: Record final result, winner, scores of an event
        // Example: Event "Football Match A" Result: "Team A won 3-2"
        // One-to-one: Each event has at most one result record
        // Cascade: Deleting event deletes its result
        builder.Entity<EventResult>()
            .HasOne(er => er.ScheduledEvent)
            .WithMany()
            .HasForeignKey(er => er.ScheduledEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional winning group reference
        // Restrict: Can't delete a group if it's marked as winner
        builder.Entity<EventResult>()
            .HasOne(er => er.WinningGroup)
            .WithMany()
            .HasForeignKey(er => er.WinningGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // TEAM HIERARCHY (TPH: Table Per Hierarchy)
        // ========================================
        // Single "teams" table with discriminator column to distinguish Static vs Ephemeral
        // Purpose: Support both persistent teams (Static) and ad-hoc groupings (Ephemeral via EventParticipantGroup)
        // StaticTeam is the only implementation currently, but structure is ready for future types
         //builder.Entity<StaticTeam>()
         //   .HasDiscriminator<TeamType>("TeamType")
         //  .HasValue<StaticTeam>(TeamType.Static);

        // STATIC TEAM MEMBER: Primary key and relationships
        // Uses inherited Id as PK. Allows transfer history - player can rejoin same team
        // Filtered unique index on (StaticTeamId, PlayerId) where LeftAt is null
        // Ensures only ONE active membership per player per team, but allows multiple historical records
        builder.Entity<StaticTeamMember>()
            .HasIndex(stm => new { stm.StaticTeamId, stm.PlayerId })
            .IsUnique()
            .HasFilter("\"LeftAt\" IS NULL");  // PostgreSQL syntax for filtered index

        // STATIC TEAM: Persistent roster for a season/tournament
        // Purpose: Teams that remain constant throughout competition
        // Example: "FC Barcelona" in "La Liga 2025" has fixed roster registered at season start
        builder.Entity<StaticTeam>()
            .HasMany(st => st.Members)
            .WithOne(stm => stm.StaticTeam)
            .HasForeignKey(stm => stm.StaticTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // STATIC TEAM MEMBER: Individual player in a static team with transfer tracking
        // Purpose: Record player's tenure, transfers, loans within a static team
        // Example: "John" joined "FC Barcelona" on 2024-01-01, left on 2025-01-15 (Transfer)
        builder.Entity<StaticTeamMember>()
            .HasOne(stm => stm.Player)
            .WithMany()
            .HasForeignKey(stm => stm.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // TEAM REFERENCE IN EVENT PARTICIPANT GROUP
        // Optional: If set, group uses Static Team's current roster (at event time)
        // If null, group uses direct EventParticipants (ephemeral/legacy)
        builder.Entity<EventParticipantGroup>()
            .HasOne(epg => epg.Team)
            .WithMany()
            .HasForeignKey(epg => epg.TeamId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}