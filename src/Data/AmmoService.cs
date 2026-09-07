using MakeReady.Models;

namespace MakeReady.Data;

public record AmmoStats(int Sessions, int FirearmMalfunctions, int MagMalfunctions)
{
    public int TotalMalfunctions => FirearmMalfunctions + MagMalfunctions;
}

public class AmmoService(JsonDataStore store)
{
    public async Task<List<Ammo>> GetAllAsync()
    {
        await store.ReadyAsync();
        return store.Ammos.OrderBy(a => a.Brand).ThenBy(a => a.Grain).ToList();
    }

    public async Task<Ammo> SaveAsync(Ammo ammo)
    {
        await store.ReadyAsync();
        if (ammo.Id == 0)
        {
            ammo.Id = store.NextId();
            store.Ammos.Add(ammo);
        }
        else
        {
            var existing = store.Ammos.First(a => a.Id == ammo.Id);
            existing.Brand = ammo.Brand;
            existing.Caliber = ammo.Caliber;
            existing.Grain = ammo.Grain;
            existing.Type = ammo.Type;
            existing.Notes = ammo.Notes;
        }
        await store.SaveAsync();
        return ammo;
    }

    public async Task DeleteAsync(int id)
    {
        await store.ReadyAsync();
        foreach (var s in store.RoundSessions.Where(s => s.AmmoId == id)) s.AmmoId = null;
        foreach (var m in store.FirearmMalfunctions.Where(m => m.AmmoId == id)) m.AmmoId = null;
        foreach (var m in store.MagazineMalfunctions.Where(m => m.AmmoId == id)) m.AmmoId = null;
        store.Ammos.RemoveAll(a => a.Id == id);
        await store.SaveAsync();
    }

    public async Task<Dictionary<int, AmmoStats>> GetStatsMapAsync()
    {
        await store.ReadyAsync();

        var sessionMap = store.RoundSessions
            .Where(s => s.AmmoId != null)
            .GroupBy(s => s.AmmoId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var fireMalMap = store.FirearmMalfunctions
            .Where(m => m.AmmoId != null)
            .GroupBy(m => m.AmmoId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var magMalMap = store.MagazineMalfunctions
            .Where(m => m.AmmoId != null)
            .GroupBy(m => m.AmmoId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

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
