using MakeReady.Models;

namespace MakeReady.Data;

public class MaintenanceService(JsonDataStore store)
{
    private const int OverdueDays       = 90;
    private const int DueSoonDays       = 15;
    private const int SessionsThreshold = 5;

    // ── Firearm query ────────────────────────────────────────────
    public async Task<List<Firearm>> GetFirearmsWithMaintenanceAsync()
    {
        await store.ReadyAsync();
        return store.Firearms.OrderBy(f => f.Make).ThenBy(f => f.Model).ToList();
    }

    public async Task<Firearm?> GetFirearmDetailAsync(int id)
    {
        await store.ReadyAsync();
        return store.Firearms.FirstOrDefault(f => f.Id == id);
    }

    // ── Schedule ─────────────────────────────────────────────────
    public async Task SaveScheduleAsync(MaintenanceSchedule schedule)
    {
        await store.ReadyAsync();
        var existing = store.MaintenanceSchedules.FirstOrDefault(s => s.FirearmId == schedule.FirearmId);
        if (existing == null)
        {
            schedule.Id = store.NextId();
            store.MaintenanceSchedules.Add(schedule);
        }
        else
        {
            existing.IntervalDays = schedule.IntervalDays;
            existing.NotificationsEnabled = schedule.NotificationsEnabled;
        }
        await store.SaveAsync();
    }

    public async Task DeleteScheduleAsync(int firearmId)
    {
        await store.ReadyAsync();
        store.MaintenanceSchedules.RemoveAll(s => s.FirearmId == firearmId);
        await store.SaveAsync();
    }

    // ── Parts ─────────────────────────────────────────────────────
    public async Task<MaintenancePart> SavePartAsync(MaintenancePart part)
    {
        await store.ReadyAsync();
        if (part.Id == 0)
        {
            part.Id = store.NextId();
            store.MaintenanceParts.Add(part);
        }
        else
        {
            var existing = store.MaintenanceParts.First(p => p.Id == part.Id);
            existing.Name = part.Name;
            existing.Brand = part.Brand;
            existing.IntervalDays = part.IntervalDays;
            existing.NotificationsEnabled = part.NotificationsEnabled;
        }
        await store.SaveAsync();
        return part;
    }

    public async Task DeletePartAsync(int id)
    {
        await store.ReadyAsync();
        foreach (var p in store.MaintenanceLogParts.Where(p => p.MaintenancePartId == id))
            p.MaintenancePartId = null;
        store.MaintenanceParts.RemoveAll(p => p.Id == id);
        await store.SaveAsync();
    }

    // ── Logs ──────────────────────────────────────────────────────
    public async Task<MaintenanceLog> LogCleaningAsync(MaintenanceLog log)
    {
        await store.ReadyAsync();
        log.Id = store.NextId();
        store.MaintenanceLogs.Add(log);
        foreach (var part in log.CleanedParts)
        {
            part.Id = store.NextId();
            part.MaintenanceLogId = log.Id;
            store.MaintenanceLogParts.Add(part);
        }
        await store.SaveAsync();
        return log;
    }

    public async Task DeleteLogAsync(int id)
    {
        await store.ReadyAsync();
        store.MaintenanceLogParts.RemoveAll(p => p.MaintenanceLogId == id);
        store.MaintenanceLogs.RemoveAll(l => l.Id == id);
        await store.SaveAsync();
    }

    // ── Status computation ────────────────────────────────────────
    public MaintenanceAlert GetFirearmAlert(Firearm f)
    {
        var lastLog     = f.MaintenanceLogs.OrderByDescending(l => l.Date).FirstOrDefault();
        var lastCleaned = lastLog?.Date;
        var daysSince   = lastCleaned.HasValue ? (int)(DateTime.UtcNow - lastCleaned.Value).TotalDays : int.MaxValue;
        var sessionsSinceClean = lastCleaned.HasValue
            ? f.Sessions.Count(s => s.Date > lastCleaned.Value)
            : f.Sessions.Count;

        if (f.Schedule != null)
        {
            var interval     = f.Schedule.IntervalDays;
            var daysUntilDue = interval - daysSince;

            if (daysUntilDue <= 0)
                return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.Overdue,
                    $"Overdue by {-daysUntilDue} day{(-daysUntilDue == 1 ? "" : "s")}",
                    lastCleaned, daysUntilDue, sessionsSinceClean);

            if (daysUntilDue <= DueSoonDays)
                return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.DueSoon,
                    $"Due in {daysUntilDue} day{(daysUntilDue == 1 ? "" : "s")}",
                    lastCleaned, daysUntilDue, sessionsSinceClean);

            return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.OK,
                $"Next cleaning in {daysUntilDue} days", lastCleaned, daysUntilDue, sessionsSinceClean);
        }
        else
        {
            // No schedule — apply heuristic rules
            if (daysSince > OverdueDays)
                return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.Overdue,
                    lastCleaned == null ? "Never cleaned" : $"Last cleaned {daysSince} days ago",
                    lastCleaned, null, sessionsSinceClean);

            if (sessionsSinceClean > SessionsThreshold)
                return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.Overdue,
                    $"{sessionsSinceClean} sessions since last cleaning",
                    lastCleaned, null, sessionsSinceClean);

            if (daysSince > OverdueDays - DueSoonDays)
                return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.DueSoon,
                    $"Last cleaned {daysSince} days ago",
                    lastCleaned, null, sessionsSinceClean);

            return new(f.Id, f.DisplayName, f.Category, MaintenanceStatusLevel.OK,
                lastCleaned == null ? "No cleaning logged yet" : $"Last cleaned {daysSince} days ago",
                lastCleaned, null, sessionsSinceClean);
        }
    }

    public PartAlert GetPartAlert(Firearm f, MaintenancePart part)
    {
        if (part.IntervalDays == null)
            return new(f.Id, f.DisplayName, part.Id, part.Name,
                MaintenanceStatusLevel.OK, "No schedule set", null, null);

        var lastLog = f.MaintenanceLogs
            .Where(l => l.CleanedParts.Any(p => p.MaintenancePartId == part.Id))
            .OrderByDescending(l => l.Date)
            .FirstOrDefault();

        var lastCleaned = lastLog?.Date;
        var daysSince   = lastCleaned.HasValue ? (int)(DateTime.UtcNow - lastCleaned.Value).TotalDays : int.MaxValue;
        var daysUntilDue = part.IntervalDays.Value - daysSince;

        if (daysUntilDue <= 0)
            return new(f.Id, f.DisplayName, part.Id, part.Name, MaintenanceStatusLevel.Overdue,
                $"Overdue by {-daysUntilDue} day{(-daysUntilDue == 1 ? "" : "s")}", lastCleaned, daysUntilDue);

        if (daysUntilDue <= DueSoonDays)
            return new(f.Id, f.DisplayName, part.Id, part.Name, MaintenanceStatusLevel.DueSoon,
                $"Due in {daysUntilDue} days", lastCleaned, daysUntilDue);

        return new(f.Id, f.DisplayName, part.Id, part.Name, MaintenanceStatusLevel.OK,
            $"Next cleaning in {daysUntilDue} days", lastCleaned, daysUntilDue);
    }

    public async Task<List<MaintenanceAlert>> GetAllAlertsAsync()
    {
        var firearms = await GetFirearmsWithMaintenanceAsync();
        return firearms.Select(GetFirearmAlert)
            .Where(a => a.Status != MaintenanceStatusLevel.OK)
            .OrderByDescending(a => a.Status)
            .ToList();
    }
}
