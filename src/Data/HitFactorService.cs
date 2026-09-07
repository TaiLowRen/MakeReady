using MakeReady.Models;

namespace MakeReady.Data;

public class HitFactorService(JsonDataStore store)
{
    public async Task<List<HitFactorSession>> GetSessionsAsync()
    {
        await store.ReadyAsync();
        return store.HitFactorSessions.OrderByDescending(s => s.Date).ToList();
    }

    public async Task<HitFactorSession?> GetSessionAsync(int id)
    {
        await store.ReadyAsync();
        return store.HitFactorSessions.FirstOrDefault(s => s.Id == id);
    }

    public async Task<HitFactorSession> SaveSessionAsync(HitFactorSession session)
    {
        await store.ReadyAsync();
        if (session.Id == 0)
        {
            session.Id = store.NextId();
            store.HitFactorSessions.Add(session);
        }
        else
        {
            var existing = store.HitFactorSessions.First(s => s.Id == session.Id);
            existing.FirearmId = session.FirearmId;
            existing.Name = session.Name;
            existing.Date = session.Date;
            existing.Location = session.Location;
            existing.Notes = session.Notes;
        }
        await store.SaveAsync();
        return session;
    }

    public async Task DeleteSessionAsync(int id)
    {
        await store.ReadyAsync();
        store.HitFactorStages.RemoveAll(s => s.HitFactorSessionId == id);
        store.HitFactorSessions.RemoveAll(s => s.Id == id);
        await store.SaveAsync();
    }

    public async Task<HitFactorStage> AddStageAsync(HitFactorStage stage)
    {
        await store.ReadyAsync();
        stage.Id = store.NextId();
        store.HitFactorStages.Add(stage);
        await store.SaveAsync();
        return stage;
    }

    public async Task DeleteStageAsync(int id)
    {
        await store.ReadyAsync();
        store.HitFactorStages.RemoveAll(s => s.Id == id);
        await store.SaveAsync();
    }
}
