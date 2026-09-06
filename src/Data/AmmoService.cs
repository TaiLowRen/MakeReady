using MakeReady.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeReady.Data;

public record AmmoStats(int Sessions, int FirearmMalfunctions, int MagMalfunctions)
{
    public int TotalMalfunctions => FirearmMalfunctions + MagMalfunctions;
}

public class AmmoService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Ammo>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Ammos.OrderBy(a => a.Brand).ThenBy(a => a.Grain).ToListAsync();
    }

    public async Task<Ammo> SaveAsync(Ammo ammo)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (ammo.Id == 0)
            db.Ammos.Add(ammo);
        else
            db.Ammos.Update(ammo);
        await db.SaveChangesAsync();
        return ammo;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.Ammos.Where(a => a.Id == id).ExecuteDeleteAsync();
    }

    public async Task<Dictionary<int, AmmoStats>> GetStatsMapAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        var sessions = await db.RoundSessions
            .Where(s => s.AmmoId != null)
            .GroupBy(s => s.AmmoId!.Value)
            .Select(g => new { AmmoId = g.Key, Count = g.Count() })
            .ToListAsync();

        var fireMal = await db.FirearmMalfunctions
            .Where(m => m.AmmoId != null)
            .GroupBy(m => m.AmmoId!.Value)
            .Select(g => new { AmmoId = g.Key, Count = g.Count() })
            .ToListAsync();

        var magMal = await db.MagazineMalfunctions
            .Where(m => m.AmmoId != null)
            .GroupBy(m => m.AmmoId!.Value)
            .Select(g => new { AmmoId = g.Key, Count = g.Count() })
            .ToListAsync();

        var sessionMap  = sessions.ToDictionary(x => x.AmmoId, x => x.Count);
        var fireMalMap  = fireMal.ToDictionary(x => x.AmmoId, x => x.Count);
        var magMalMap   = magMal.ToDictionary(x => x.AmmoId, x => x.Count);

        var allIds = sessionMap.Keys.Union(fireMalMap.Keys).Union(magMalMap.Keys).ToHashSet();
        return allIds.ToDictionary(
            id => id,
            id => new AmmoStats(
                sessionMap.GetValueOrDefault(id),
                fireMalMap.GetValueOrDefault(id),
                magMalMap.GetValueOrDefault(id)
            ));
    }
}
