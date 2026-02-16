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

        builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Activity>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<ScheduledEvent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Player>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<Permission>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<EventParticipantGroup>().HasQueryFilter(epg => !epg.IsDeleted);
        builder.Entity<Membership>().HasQueryFilter(m => !m.AppUser.IsDeleted);

        // Identity indexes
        builder.Entity<AppUser>().HasIndex(u => u.AwsSubject).IsUnique();

        // Useful indexes
        builder.Entity<ScheduledEvent>().HasIndex(e => e.TenantId);
        builder.Entity<Player>().HasIndex(p => new { p.TenantId, p.AppUserId }).IsUnique();
        builder.Entity<Membership>().HasIndex(m => new { m.AppUserId, m.TenantId });

        // === MANY-TO-MANY RELATIONSHIPS ===

        // Many-to-many: AppUser <-> Tenant through Membership
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

        builder.Entity<Membership>()
            .HasOne(m => m.Role)
            .WithMany(r => r.Memberships)
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many: Role <-> Permission through RolePermission
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

        // === ONE-TO-MANY RELATIONSHIPS ===

        // One-to-many: Activity -> Tenant
        builder.Entity<Tenant>()
            .HasOne(t => t.Activity)
            .WithMany(a => a.Tenants)
            .HasForeignKey(t => t.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: Tenant -> Role (default role)
        builder.Entity<Tenant>()
            .HasOne(t => t.DefaultRole)
            .WithMany()
            .HasForeignKey(t => t.DefaultRoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many: Tenant -> ScheduledEvent
        builder.Entity<ScheduledEvent>()
            .HasOne(e => e.Tenant)
            .WithMany(t => t.ScheduledEvents)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Tenant -> Player
        builder.Entity<Player>()
            .HasOne(p => p.Tenant)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: AppUser -> Player
        builder.Entity<Player>()
            .HasOne(p => p.AppUser)
            .WithMany()
            .HasForeignKey(p => p.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: ScheduledEvent -> EventParticipantGroup
        builder.Entity<EventParticipantGroup>()
            .HasOne(epg => epg.ScheduledEvent)
            .WithMany()
            .HasForeignKey(epg => epg.ScheduledEventId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: EventParticipantGroup -> EventParticipant
        builder.Entity<EventParticipant>()
            .HasOne(ep => ep.EventParticipantGroup)
            .WithMany()
            .HasForeignKey(ep => ep.EventParticipantGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many: Player -> EventParticipant
        builder.Entity<EventParticipant>()
            .HasOne(ep => ep.Player)
            .WithMany()
            .HasForeignKey(ep => ep.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventParticipant>()
            .HasKey(m => new { m.EventParticipantGroupId, m.PlayerId });  
    }
}