using MakeReady.Models;

namespace MakeReady.Data;

public class DrillService(JsonDataStore store)
{
    public async Task<List<Drill>> GetDrillsAsync()
    {
        await store.ReadyAsync();
        return store.Drills.OrderBy(d => d.Name).ToList();
    }

    public async Task<Drill?> GetDrillAsync(int id)
    {
        await store.ReadyAsync();
        return store.Drills.FirstOrDefault(d => d.Id == id);
    }

    public async Task<Drill> SaveDrillAsync(Drill drill, List<DrillTarget> targets)
    {
        await store.ReadyAsync();

        if (drill.Id == 0)
        {
            drill.Id = store.NextId();
            store.Drills.Add(drill);
        }
        else
        {
            var existing = store.Drills.First(d => d.Id == drill.Id);
            existing.Name = drill.Name;
            existing.Notes = drill.Notes;
            existing.RandomDelayMinSeconds = drill.RandomDelayMinSeconds;
            existing.RandomDelayMaxSeconds = drill.RandomDelayMaxSeconds;
            existing.ParTimeSeconds = drill.ParTimeSeconds;
        }

        store.DrillTargets.RemoveAll(t => t.DrillId == drill.Id);
        var order = 0;
        foreach (var t in targets)
        {
            t.Id = store.NextId();
            t.DrillId = drill.Id;
            t.OrderIndex = order++;
            store.DrillTargets.Add(t);
        }

        await store.SaveAsync();
        return drill;
    }

    public async Task DeleteDrillAsync(int id)
    {
        await store.ReadyAsync();
        store.DrillTargets.RemoveAll(t => t.DrillId == id);
        store.DrillSessions.RemoveAll(s => s.DrillId == id);
        store.Drills.RemoveAll(d => d.Id == id);
        await store.SaveAsync();
    }

    public async Task<DrillSession> LogSessionAsync(DrillSession session)
    {
        await store.ReadyAsync();
        session.Id = store.NextId();
        store.DrillSessions.Add(session);
        await store.SaveAsync();
        return session;
    }
}
