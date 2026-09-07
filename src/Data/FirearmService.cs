using MakeReady.Models;

namespace MakeReady.Data;

public class FirearmService(JsonDataStore store)
{
    // Firearms
    public async Task<List<Firearm>> GetFirearmsAsync()
    {
        await store.ReadyAsync();
        return store.Firearms.OrderBy(f => f.Make).ThenBy(f => f.Model).ToList();
    }

    public async Task<Firearm?> GetFirearmAsync(int id)
    {
        await store.ReadyAsync();
        return store.Firearms.FirstOrDefault(f => f.Id == id);
    }

    public async Task<Firearm> SaveFirearmAsync(Firearm firearm)
    {
        await store.ReadyAsync();
        if (firearm.Id == 0)
        {
            firearm.Id = store.NextId();
            store.Firearms.Add(firearm);
        }
        else
        {
            var existing = store.Firearms.First(f => f.Id == firearm.Id);
            existing.Make = firearm.Make;
            existing.Model = firearm.Model;
            existing.Notes = firearm.Notes;
            existing.Category = firearm.Category;
        }
        await store.SaveAsync();
        return firearm;
    }

    public async Task DeleteFirearmAsync(int id)
    {
        await store.ReadyAsync();
        var magIds = store.Magazines.Where(m => m.FirearmId == id).Select(m => m.Id).ToHashSet();
        store.MagazineModifications.RemoveAll(m => magIds.Contains(m.MagazineId));
        store.MagazineMalfunctions.RemoveAll(m => magIds.Contains(m.MagazineId));
        store.Magazines.RemoveAll(m => m.FirearmId == id);
        store.Modifications.RemoveAll(m => m.FirearmId == id);
        store.RoundSessions.RemoveAll(s => s.FirearmId == id);
        store.FirearmMalfunctions.RemoveAll(m => m.FirearmId == id);
        store.MaintenanceSchedules.RemoveAll(s => s.FirearmId == id);
        store.MaintenanceParts.RemoveAll(p => p.FirearmId == id);
        var logIds = store.MaintenanceLogs.Where(l => l.FirearmId == id).Select(l => l.Id).ToHashSet();
        store.MaintenanceLogParts.RemoveAll(p => logIds.Contains(p.MaintenanceLogId));
        store.MaintenanceLogs.RemoveAll(l => l.FirearmId == id);
        foreach (var hf in store.HitFactorSessions.Where(h => h.FirearmId == id))
            hf.FirearmId = null;
        store.Firearms.RemoveAll(f => f.Id == id);
        await store.SaveAsync();
    }

    // Modifications
    public async Task<Modification> AddModificationAsync(Modification mod)
    {
        await store.ReadyAsync();
        mod.Id = store.NextId();
        store.Modifications.Add(mod);
        await store.SaveAsync();
        return mod;
    }

    public async Task DeleteModificationAsync(int id)
    {
        await store.ReadyAsync();
        store.Modifications.RemoveAll(m => m.Id == id);
        await store.SaveAsync();
    }

    // Magazines
    public async Task<Magazine?> GetMagazineAsync(int id)
    {
        await store.ReadyAsync();
        return store.Magazines.FirstOrDefault(m => m.Id == id);
    }

    public async Task<Magazine> SaveMagazineAsync(Magazine mag)
    {
        await store.ReadyAsync();
        if (mag.Id == 0)
        {
            mag.Id = store.NextId();
            store.Magazines.Add(mag);
        }
        else
        {
            var existing = store.Magazines.First(m => m.Id == mag.Id);
            existing.Label = mag.Label;
            existing.Capacity = mag.Capacity;
            existing.TotalRoundsFired = mag.TotalRoundsFired;
        }
        await store.SaveAsync();
        return mag;
    }

    public async Task DeleteMagazineAsync(int id)
    {
        await store.ReadyAsync();
        store.MagazineModifications.RemoveAll(m => m.MagazineId == id);
        store.MagazineMalfunctions.RemoveAll(m => m.MagazineId == id);
        foreach (var s in store.RoundSessions.Where(s => s.MagazineId == id))
            s.MagazineId = null;
        store.Magazines.RemoveAll(m => m.Id == id);
        await store.SaveAsync();
    }

    // Magazine Modifications
    public async Task<MagazineModification> AddMagazineModificationAsync(MagazineModification mod)
    {
        await store.ReadyAsync();
        mod.Id = store.NextId();
        store.MagazineModifications.Add(mod);
        await store.SaveAsync();
        return mod;
    }

    public async Task DeleteMagazineModificationAsync(int id)
    {
        await store.ReadyAsync();
        store.MagazineModifications.RemoveAll(m => m.Id == id);
        await store.SaveAsync();
    }

    // Magazine Malfunctions
    public async Task<MagazineMalfunction> AddMalfunctionAsync(MagazineMalfunction malfunction)
    {
        await store.ReadyAsync();
        malfunction.Id = store.NextId();
        store.MagazineMalfunctions.Add(malfunction);
        await store.SaveAsync();
        return malfunction;
    }

    public async Task DeleteMalfunctionAsync(int id)
    {
        await store.ReadyAsync();
        store.MagazineMalfunctions.RemoveAll(m => m.Id == id);
        await store.SaveAsync();
    }

    // Firearm Malfunctions
    public async Task<FirearmMalfunction> AddFirearmMalfunctionAsync(FirearmMalfunction malfunction)
    {
        await store.ReadyAsync();
        malfunction.Id = store.NextId();
        store.FirearmMalfunctions.Add(malfunction);
        await store.SaveAsync();
        return malfunction;
    }

    public async Task DeleteFirearmMalfunctionAsync(int id)
    {
        await store.ReadyAsync();
        store.FirearmMalfunctions.RemoveAll(m => m.Id == id);
        await store.SaveAsync();
    }

    // Sessions
    public async Task<RoundSession> LogSessionAsync(RoundSession session)
    {
        await store.ReadyAsync();
        session.Id = store.NextId();
        store.RoundSessions.Add(session);

        if (session.MagazineId.HasValue)
        {
            var mag = store.Magazines.FirstOrDefault(m => m.Id == session.MagazineId.Value);
            if (mag != null)
                mag.TotalRoundsFired += session.RoundsFired;
        }

        await store.SaveAsync();
        return session;
    }

    public async Task DeleteSessionAsync(int id)
    {
        await store.ReadyAsync();
        store.RoundSessions.RemoveAll(s => s.Id == id);
        await store.SaveAsync();
    }
}
