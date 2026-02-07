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
    public DbSet<League> Leagues => Set<League>();
    public DbSet<ScheduledEvent> ScheduledEvents => Set<ScheduledEvent>();
    public DbSet<AppUserLeague> AppUserLeagues => Set<AppUserLeague>();

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

        // Primary key column naming: Id -> {EntityName}Id
        builder.Entity<AppUser>().Property(u => u.Id).HasColumnName("AppUserId");
        builder.Entity<Activity>().Property(a => a.Id).HasColumnName("ActivityId");
        builder.Entity<League>().Property(l => l.Id).HasColumnName("LeagueId");
        builder.Entity<ScheduledEvent>().Property(e => e.Id).HasColumnName("ScheduledEventId");

        builder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<Activity>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<League>().HasQueryFilter(l => !l.IsDeleted);
        builder.Entity<ScheduledEvent>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<AppUserLeague>().HasQueryFilter(ul => !ul.AppUser.IsDeleted);

        // Identity indexes
        builder.Entity<AppUser>().HasIndex(u => u.AwsSubject).IsUnique();

        // Useful indexes
        builder.Entity<ScheduledEvent>().HasIndex(e => e.LeagueId);

        // Associations
        // Many-to-many between League and AppUser through AppUserLeague
        builder.Entity<AppUserLeague>()
            .HasKey(al => new { al.AppUserId, al.LeagueId });

        builder.Entity<AppUserLeague>()
            .HasOne(al => al.AppUser)
            .WithMany(u => u.AppUserLeagues)
            .HasForeignKey(al => al.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AppUserLeague>()
            .HasOne(al => al.League)
            .WithMany(l => l.AppUserLeagues)
            .HasForeignKey(al => al.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many between Activity and League
        builder.Entity<League>()
            .HasOne(l => l.Activity)
            .WithMany(a => a.Leagues)
            .HasForeignKey(l => l.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many between League and ScheduledEvent
        builder.Entity<ScheduledEvent>()
            .HasOne(e => e.League)
            .WithMany(l => l.Events)
            .HasForeignKey(e => e.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}