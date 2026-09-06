using MakeReady.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeReady.Data;

public class FirearmService(IDbContextFactory<AppDbContext> factory)
{
    // Firearms
    public async Task<List<Firearm>> GetFirearmsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Firearms
            .Include(f => f.Sessions)
            .OrderBy(f => f.Make).ThenBy(f => f.Model)
            .ToListAsync();
    }

    public async Task<Firearm?> GetFirearmAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Firearms
            .Include(f => f.Modifications)
            .Include(f => f.Magazines)
            .Include(f => f.Sessions).ThenInclude(s => s.Magazine)
            .Include(f => f.Sessions).ThenInclude(s => s.Ammo)
            .Include(f => f.Malfunctions).ThenInclude(m => m.Ammo)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Firearm> SaveFirearmAsync(Firearm firearm)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (firearm.Id == 0)
            db.Firearms.Add(firearm);
        else
            db.Firearms.Update(firearm);
        await db.SaveChangesAsync();
        return firearm;
    }

    public async Task DeleteFirearmAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Firearms.Where(f => f.Id == id).ExecuteDeleteAsync();
    }

    // Modifications
    public async Task<Modification> AddModificationAsync(Modification mod)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.Modifications.Add(mod);
        await db.SaveChangesAsync();
        return mod;
    }

    public async Task DeleteModificationAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Modifications.Where(m => m.Id == id).ExecuteDeleteAsync();
    }

    // Magazines
    public async Task<Magazine?> GetMagazineAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Magazines
            .Include(m => m.Modifications)
            .Include(m => m.Malfunctions).ThenInclude(mf => mf.Ammo)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Magazine> SaveMagazineAsync(Magazine mag)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (mag.Id == 0)
            db.Magazines.Add(mag);
        else
            db.Magazines.Update(mag);
        await db.SaveChangesAsync();
        return mag;
    }

    public async Task DeleteMagazineAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Magazines.Where(m => m.Id == id).ExecuteDeleteAsync();
    }

    // Magazine Modifications
    public async Task<MagazineModification> AddMagazineModificationAsync(MagazineModification mod)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.MagazineModifications.Add(mod);
        await db.SaveChangesAsync();
        return mod;
    }

    public async Task DeleteMagazineModificationAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.MagazineModifications.Where(m => m.Id == id).ExecuteDeleteAsync();
    }

    // Magazine Malfunctions
    public async Task<MagazineMalfunction> AddMalfunctionAsync(MagazineMalfunction malfunction)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.MagazineMalfunctions.Add(malfunction);
        await db.SaveChangesAsync();
        return malfunction;
    }

    public async Task DeleteMalfunctionAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.MagazineMalfunctions.Where(m => m.Id == id).ExecuteDeleteAsync();
    }

    // Firearm Malfunctions
    public async Task<FirearmMalfunction> AddFirearmMalfunctionAsync(FirearmMalfunction malfunction)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.FirearmMalfunctions.Add(malfunction);
        await db.SaveChangesAsync();
        return malfunction;
    }

    public async Task DeleteFirearmMalfunctionAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.FirearmMalfunctions.Where(m => m.Id == id).ExecuteDeleteAsync();
    }

    // Sessions
    public async Task<RoundSession> LogSessionAsync(RoundSession session)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.RoundSessions.Add(session);

        if (session.MagazineId.HasValue)
        {
            var mag = await db.Magazines.FindAsync(session.MagazineId.Value);
            if (mag != null)
                mag.TotalRoundsFired += session.RoundsFired;
        }

        await db.SaveChangesAsync();
        return session;
    }

    public async Task DeleteSessionAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.RoundSessions.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
}
