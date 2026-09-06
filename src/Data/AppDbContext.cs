using MakeReady.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeReady.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ammo> Ammos { get; set; }
    public DbSet<Firearm> Firearms { get; set; }
    public DbSet<Modification> Modifications { get; set; }
    public DbSet<Magazine> Magazines { get; set; }
    public DbSet<RoundSession> RoundSessions { get; set; }
    public DbSet<MagazineModification> MagazineModifications { get; set; }
    public DbSet<MagazineMalfunction> MagazineMalfunctions { get; set; }
    public DbSet<FirearmMalfunction> FirearmMalfunctions { get; set; }
    public DbSet<HitFactorSession> HitFactorSessions { get; set; }
    public DbSet<HitFactorStage> HitFactorStages { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenancePart> MaintenanceParts { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }
    public DbSet<MaintenanceLogPart> MaintenanceLogParts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.Modifications)
            .WithOne(m => m.Firearm)
            .HasForeignKey(m => m.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.Magazines)
            .WithOne(m => m.Firearm)
            .HasForeignKey(m => m.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.Sessions)
            .WithOne(s => s.Firearm)
            .HasForeignKey(s => s.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoundSession>()
            .HasOne(s => s.Magazine)
            .WithMany()
            .HasForeignKey(s => s.MagazineId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Magazine>()
            .HasMany(m => m.Modifications)
            .WithOne(mm => mm.Magazine)
            .HasForeignKey(mm => mm.MagazineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Magazine>()
            .HasMany(m => m.Malfunctions)
            .WithOne(mf => mf.Magazine)
            .HasForeignKey(mf => mf.MagazineId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.Malfunctions)
            .WithOne(mf => mf.Firearm)
            .HasForeignKey(mf => mf.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HitFactorSession>()
            .HasOne(s => s.Firearm)
            .WithMany()
            .HasForeignKey(s => s.FirearmId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HitFactorSession>()
            .HasMany(s => s.Stages)
            .WithOne(st => st.Session)
            .HasForeignKey(st => st.HitFactorSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasOne(f => f.Schedule)
            .WithOne(s => s.Firearm)
            .HasForeignKey<MaintenanceSchedule>(s => s.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.Parts)
            .WithOne(p => p.Firearm)
            .HasForeignKey(p => p.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Firearm>()
            .HasMany(f => f.MaintenanceLogs)
            .WithOne(l => l.Firearm)
            .HasForeignKey(l => l.FirearmId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MaintenanceLog>()
            .HasMany(l => l.CleanedParts)
            .WithOne(p => p.MaintenanceLog)
            .HasForeignKey(p => p.MaintenanceLogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
