using MakeReady.Models;
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

public class ExportService(JsonDataStore store)
{
    public async Task<List<Firearm>> GetAllFirearmsAsync()
    {
        await store.ReadyAsync();
        return store.Firearms.OrderBy(f => f.Make).ThenBy(f => f.Model).ToList();
    }

    public async Task<ExportData> GetDataAsync(ExportFilter filter, ExportSections sections)
    {
        await store.ReadyAsync();

        bool hasFirearmFilter = (filter.Categories?.Count ?? 0) > 0 || (filter.FirearmIds?.Count ?? 0) > 0;

        var firearmsQ = store.Firearms.AsEnumerable();
        if (filter.Categories?.Count > 0)
            firearmsQ = firearmsQ.Where(f => filter.Categories.Contains(f.Category));
        if (filter.FirearmIds?.Count > 0)
            firearmsQ = firearmsQ.Where(f => filter.FirearmIds.Contains(f.Id));
        var firearms = firearmsQ.OrderBy(f => f.Make).ToList();
        var ids = firearms.Select(f => f.Id).ToHashSet();

        DateTime? fromUtc = filter.FromDate.HasValue ? DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Local).ToUniversalTime() : null;
        DateTime? toUtc   = filter.ToDate.HasValue   ? DateTime.SpecifyKind(filter.ToDate.Value.AddDays(1).AddSeconds(-1), DateTimeKind.Local).ToUniversalTime() : null;

        // Sessions
        List<RoundSession> sessions = new();
        if (sections.RoundSessions)
        {
            var q = store.RoundSessions.AsEnumerable();
            if (hasFirearmFilter) q = q.Where(s => ids.Contains(s.FirearmId));
            if (fromUtc.HasValue) q = q.Where(s => s.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(s => s.Date <= toUtc.Value);
            if (filter.MinRounds.HasValue) q = q.Where(s => s.RoundsFired >= filter.MinRounds.Value);
            if (filter.MaxRounds.HasValue) q = q.Where(s => s.RoundsFired <= filter.MaxRounds.Value);
            sessions = q.OrderBy(s => s.Date).ToList();
        }

        // Firearm malfunctions
        List<FirearmMalfunction> fireMal = new();
        if (sections.Malfunctions)
        {
            var q = store.FirearmMalfunctions.AsEnumerable();
            if (hasFirearmFilter) q = q.Where(m => ids.Contains(m.FirearmId));
            if (fromUtc.HasValue) q = q.Where(m => m.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(m => m.Date <= toUtc.Value);
            fireMal = q.OrderBy(m => m.Date).ToList();
        }

        // Magazine malfunctions
        List<MagazineMalfunction> magMal = new();
        if (sections.Malfunctions)
        {
            var q = store.MagazineMalfunctions.AsEnumerable();
            if (hasFirearmFilter) q = q.Where(m => ids.Contains(m.Magazine.FirearmId));
            if (fromUtc.HasValue) q = q.Where(m => m.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(m => m.Date <= toUtc.Value);
            magMal = q.OrderBy(m => m.Date).ToList();
        }

        // Hit factor
        List<HitFactorSession> hfSessions = new();
        if (sections.HitFactor)
        {
            var all = store.HitFactorSessions.OrderBy(s => s.Date).AsEnumerable();
            if (hasFirearmFilter) all = all.Where(s => s.FirearmId == null || ids.Contains(s.FirearmId.Value));
            if (fromUtc.HasValue) all = all.Where(s => s.Date >= fromUtc.Value);
            if (toUtc.HasValue)   all = all.Where(s => s.Date <= toUtc.Value);
            hfSessions = all.ToList();
        }

        // Maintenance
        List<MaintenanceLog> maintLogs = new();
        if (sections.Maintenance)
        {
            var q = store.MaintenanceLogs.AsEnumerable();
            if (hasFirearmFilter) q = q.Where(l => ids.Contains(l.FirearmId));
            if (fromUtc.HasValue) q = q.Where(l => l.Date >= fromUtc.Value);
            if (toUtc.HasValue)   q = q.Where(l => l.Date <= toUtc.Value);
            maintLogs = q.OrderBy(l => l.Date).ToList();
        }

        return new(firearms.Count > 0 ? firearms : store.Firearms.OrderBy(f => f.Make).ToList(),
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
