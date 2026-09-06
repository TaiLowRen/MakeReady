using MakeReady.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeReady.Data;

public class HitFactorService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<HitFactorSession>> GetSessionsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.HitFactorSessions
            .Include(s => s.Firearm)
            .Include(s => s.Stages)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<HitFactorSession?> GetSessionAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.HitFactorSessions
            .Include(s => s.Firearm)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<HitFactorSession> SaveSessionAsync(HitFactorSession session)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (session.Id == 0)
            db.HitFactorSessions.Add(session);
        else
            db.HitFactorSessions.Update(session);
        await db.SaveChangesAsync();
        return session;
    }

    public async Task DeleteSessionAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.HitFactorSessions.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task<HitFactorStage> AddStageAsync(HitFactorStage stage)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.HitFactorStages.Add(stage);
        await db.SaveChangesAsync();
        return stage;
    }

    public async Task DeleteStageAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        await db.HitFactorStages.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
}
