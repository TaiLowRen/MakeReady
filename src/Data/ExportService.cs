using MakeReady.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MakeReady.Data;

public record ExportFilter
{
    public DateTime? FromDate   { get; init; }
    public DateTime? ToDate     { get; init; }
    public List<int>? FirearmIds           { get; init; }
    public List<FirearmCategory>? Categories { get; init; }
    public int? MinRounds { get; init; }
    public int? MaxRounds { get; init; }
}

public record ExportSections
{
    public bool RoundSessions { get; set; } = true;
    public bool Malfunctions  { get; set; } = true;
    public bool HitFactor     { get; set; } = true;
    public bool Maintenance   { get; set; } = true;
}

public record ExportData(
    List<Firearm>              Firearms,
    List<RoundSession>         Sessions,
    List<FirearmMalfunction>   FirearmMalfunctions,
    List<MagazineMalfunction>  MagMalfunctions,
    List<HitFactorSession>     HitFactorSessions,
    List<MaintenanceLog>       MaintenanceLogs
)
{
    public int TotalRounds      => Sessions.Sum(s => s.RoundsFired);
    public int TotalMalfunctions => FirearmMalfunctions.Count + MagMalfunctions.Count;
}

public class ExportService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<Firearm>> GetAllFirearmsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Firearms.OrderBy(f => f.Make).ThenBy(f => f.Model).ToListAsync();
    }

    public async Task<ExportData> GetDataAsync(ExportFilter filter, ExportSections sections)
    {
        await using var db = await factory.CreateDbContextAsync();

        bool hasFirearmFilter = (filter.Categories?.Count ?? 0) > 0 || (filter.FirearmIds?.Count ?? 0) > 0;

        var firearmsQ = db.Firearms.AsQueryable();
        if (filter.Categories?.Count > 0)
            firearmsQ = firearmsQ.Where(f => filter.Categories.Contains(f.Category));
        if (filter.FirearmIds?.Count > 0)
            firearmsQ = firearmsQ.Where(f => filter.FirearmIds.Contains(f.Id));
        var firearms = await firearmsQ.OrderBy(f => f.Make).ToListAsync();
        var ids = firearms.Select(f => f.Id).ToHashSet();

        DateTime? fromUtc = filter.FromDate.HasValue ? DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Local).ToUniversalTime() : null;
        DateTime? toUtc   = filter.ToDate.HasValue   ? DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1).AddSeconds(-1), DateTimeKind.Local).ToUniversalTime() : null;

        // Sessions
        List<RoundSession> sessions = new();
        if (sections.RoundSessions)
        {
            var q = db.RoundSessions.Include(s => s.Firearm).Include(s => s.Magazine).AsQueryable();
            if (hasFirearmFilter) q = q.Where(s => ids.Contains(s.FirearmId));
            if (fromUtc.HasValue) q = q.Where(s => s.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(s => s.Date <= toUtc.Value);
            if (filter.MinRounds.HasValue) q = q.Where(s => s.RoundsFired >= filter.MinRounds.Value);
            if (filter.MaxRounds.HasValue) q = q.Where(s => s.RoundsFired <= filter.MaxRounds.Value);
            sessions = await q.OrderBy(s => s.Date).ToListAsync();
        }

        // Firearm malfunctions
        List<FirearmMalfunction> fireMal = new();
        if (sections.Malfunctions)
        {
            var q = db.FirearmMalfunctions.Include(m => m.Firearm).AsQueryable();
            if (hasFirearmFilter) q = q.Where(m => ids.Contains(m.FirearmId));
            if (fromUtc.HasValue) q = q.Where(m => m.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(m => m.Date <= toUtc.Value);
            fireMal = await q.OrderBy(m => m.Date).ToListAsync();
        }

        // Magazine malfunctions
        List<MagazineMalfunction> magMal = new();
        if (sections.Malfunctions)
        {
            var q = db.MagazineMalfunctions.Include(m => m.Magazine).ThenInclude(m => m.Firearm).AsQueryable();
            if (hasFirearmFilter) q = q.Where(m => ids.Contains(m.Magazine.FirearmId));
            if (fromUtc.HasValue) q = q.Where(m => m.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(m => m.Date <= toUtc.Value);
            magMal = await q.OrderBy(m => m.Date).ToListAsync();
        }

        // Hit factor
        List<HitFactorSession> hfSessions = new();
        if (sections.HitFactor)
        {
            var all = await db.HitFactorSessions.Include(s => s.Stages).Include(s => s.Firearm).OrderBy(s => s.Date).ToListAsync();
            if (hasFirearmFilter) all = all.Where(s => s.FirearmId == null || ids.Contains(s.FirearmId.Value)).ToList();
            if (fromUtc.HasValue) all = all.Where(s => s.Date >= fromUtc.Value).ToList();
            if (toUtc.HasValue)   all = all.Where(s => s.Date <= toUtc.Value).ToList();
            hfSessions = all;
        }

        // Maintenance
        List<MaintenanceLog> maintLogs = new();
        if (sections.Maintenance)
        {
            var q = db.MaintenanceLogs.Include(l => l.CleanedParts).Include(l => l.Firearm).AsQueryable();
            if (hasFirearmFilter) q = q.Where(l => ids.Contains(l.FirearmId));
            if (fromUtc.HasValue) q = q.Where(l => l.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(l => l.Date <= toUtc.Value);
            maintLogs = await q.OrderBy(l => l.Date).ToListAsync();
        }

        return new(firearms.Count > 0 ? firearms : await db.Firearms.OrderBy(f => f.Make).ToListAsync(),
                   sessions, fireMal, magMal, hfSessions, maintLogs);
    }

    public string BuildCsv(ExportData d, ExportSections s)
    {
        var sb = new StringBuilder();

        if (s.RoundSessions && d.Sessions.Count > 0)
        {
            sb.AppendLine("ROUND SESSIONS");
            sb.AppendLine("Date,Firearm,Magazine,Rounds Fired,Mode,Notes");
            foreach (var r in d.Sessions)
                sb.AppendLine($"{r.Date.ToLocalTime():yyyy-MM-dd},{E(r.Firearm?.DisplayName ?? "")},{E(r.Magazine?.Label ?? "")},{r.RoundsFired},{r.Mode},{E(r.Notes ?? "")}");
            sb.AppendLine();
        }

        if (s.Malfunctions && d.TotalMalfunctions > 0)
        {
            sb.AppendLine("MALFUNCTIONS");
            sb.AppendLine("Date,Firearm,Source,Type,Notes");
            foreach (var m in d.FirearmMalfunctions)
                sb.AppendLine($"{m.Date.ToLocalTime():yyyy-MM-dd},{E(m.Firearm?.DisplayName ?? "")},Firearm,{m.TypeLabel},{E(m.Notes ?? "")}");
            foreach (var m in d.MagMalfunctions)
                sb.AppendLine($"{m.Date.ToLocalTime():yyyy-MM-dd},{E(m.Magazine?.Firearm?.DisplayName ?? "")},Magazine ({E(m.Magazine?.Label ?? "")}),{m.TypeLabel},{E(m.Notes ?? "")}");
            sb.AppendLine();
        }

        if (s.HitFactor && d.HitFactorSessions.Count > 0)
        {
            sb.AppendLine("HIT FACTOR");
            sb.AppendLine("Date,Session,Firearm,Stage,A,C,D,Misses,NoShoots,Procedurals,Time,HitFactor,Class");
            foreach (var sess in d.HitFactorSessions)
                foreach (var st in sess.Stages)
                    sb.AppendLine($"{sess.Date.ToLocalTime():yyyy-MM-dd},{E(sess.Name)},{E(sess.Firearm?.DisplayName ?? "")},{E(st.StageName ?? "")},{st.AHits},{st.CHits},{st.DHits},{st.Misses},{st.NoShoots},{st.Procedurals},{st.Time:F2},{st.HitFactor},{st.HitFactorClass}");
            sb.AppendLine();
        }

        if (s.Maintenance && d.MaintenanceLogs.Count > 0)
        {
            sb.AppendLine("MAINTENANCE LOGS");
            sb.AppendLine("Date,Firearm,Parts Cleaned,Notes");
            foreach (var l in d.MaintenanceLogs)
                sb.AppendLine($"{l.Date.ToLocalTime():yyyy-MM-dd},{E(l.Firearm?.DisplayName ?? "")},{E(string.Join(" | ", l.CleanedParts.Select(p => p.PartName)))},{E(l.Notes ?? "")}");
        }

        return sb.ToString();
    }

    static string E(string s) =>
        s.Contains(',') || s.Contains('"') || s.Contains('\n')
            ? $"\"{s.Replace("\"", "\"\"")}\"" : s;
}
