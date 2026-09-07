using System.Text.Json;
using System.Text.Json.Serialization;
using MakeReady.Models;

namespace MakeReady.Data;

// Flat, on-disk shape: one JSON object holding every entity as a flat list (no
// navigation properties, since the model classes now [JsonIgnore] those). Navigation
// properties get wired back up in memory after load (see Hydrate below) -- this
// replaces the relational database entirely, since native SQLite has proven
// unreliable at launch on iOS regardless of provider or when it's initialized.
class StoreFile
{
    public List<Firearm> Firearms { get; set; } = new();
    public List<Modification> Modifications { get; set; } = new();
    public List<Magazine> Magazines { get; set; } = new();
    public List<MagazineModification> MagazineModifications { get; set; } = new();
    public List<MagazineMalfunction> MagazineMalfunctions { get; set; } = new();
    public List<FirearmMalfunction> FirearmMalfunctions { get; set; } = new();
    public List<RoundSession> RoundSessions { get; set; } = new();
    public List<Ammo> Ammos { get; set; } = new();
    public List<HitFactorSession> HitFactorSessions { get; set; } = new();
    public List<HitFactorStage> HitFactorStages { get; set; } = new();
    public List<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new();
    public List<MaintenancePart> MaintenanceParts { get; set; } = new();
    public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();
    public List<MaintenanceLogPart> MaintenanceLogParts { get; set; } = new();
    public List<Drill> Drills { get; set; } = new();
    public List<DrillTarget> DrillTargets { get; set; } = new();
    public List<DrillSession> DrillSessions { get; set; } = new();
}

public class JsonDataStore
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    readonly string _filePath;
    readonly SemaphoreSlim _lock = new(1, 1);
    readonly Task _ready;

    StoreFile _data = new();
    int _nextId = 1;

    public JsonDataStore(string filePath)
    {
        _filePath = filePath;
        _ready = Task.Run(LoadAsync);
    }

    public List<Firearm> Firearms => _data.Firearms;
    public List<Modification> Modifications => _data.Modifications;
    public List<Magazine> Magazines => _data.Magazines;
    public List<MagazineModification> MagazineModifications => _data.MagazineModifications;
    public List<MagazineMalfunction> MagazineMalfunctions => _data.MagazineMalfunctions;
    public List<FirearmMalfunction> FirearmMalfunctions => _data.FirearmMalfunctions;
    public List<RoundSession> RoundSessions => _data.RoundSessions;
    public List<Ammo> Ammos => _data.Ammos;
    public List<HitFactorSession> HitFactorSessions => _data.HitFactorSessions;
    public List<HitFactorStage> HitFactorStages => _data.HitFactorStages;
    public List<MaintenanceSchedule> MaintenanceSchedules => _data.MaintenanceSchedules;
    public List<MaintenancePart> MaintenanceParts => _data.MaintenanceParts;
    public List<MaintenanceLog> MaintenanceLogs => _data.MaintenanceLogs;
    public List<MaintenanceLogPart> MaintenanceLogParts => _data.MaintenanceLogParts;
    public List<Drill> Drills => _data.Drills;
    public List<DrillTarget> DrillTargets => _data.DrillTargets;
    public List<DrillSession> DrillSessions => _data.DrillSessions;

    public async Task ReadyAsync() => await _ready;

    public int NextId() => _nextId++;

    async Task LoadAsync()
    {
        await Task.Yield();

        await _lock.WaitAsync();
        try
        {
            if (File.Exists(_filePath))
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if (!string.IsNullOrWhiteSpace(json))
                    _data = JsonSerializer.Deserialize<StoreFile>(json, JsonOptions) ?? new StoreFile();
            }

            Hydrate();

            _nextId = new[]
            {
                MaxId(_data.Firearms), MaxId(_data.Modifications), MaxId(_data.Magazines),
                MaxId(_data.MagazineModifications), MaxId(_data.MagazineMalfunctions),
                MaxId(_data.FirearmMalfunctions), MaxId(_data.RoundSessions), MaxId(_data.Ammos),
                MaxId(_data.HitFactorSessions), MaxId(_data.HitFactorStages),
                MaxId(_data.MaintenanceSchedules), MaxId(_data.MaintenanceParts),
                MaxId(_data.MaintenanceLogs), MaxId(_data.MaintenanceLogParts),
                MaxId(_data.Drills), MaxId(_data.DrillTargets), MaxId(_data.DrillSessions),
            }.Max() + 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    static int MaxId<T>(List<T> list) where T : class
    {
        var idProp = typeof(T).GetProperty("Id");
        return list.Count == 0 ? 0 : list.Max(x => (int)idProp!.GetValue(x)!);
    }

    // Wires every navigation property (both directions) from the flat, FK-based
    // lists. Always fully hydrates everything rather than mimicking selective
    // EF Include() chains -- data volumes here are small enough that this is free.
    void Hydrate()
    {
        var ammoById = _data.Ammos.ToDictionary(a => a.Id);
        var magazineById = _data.Magazines.ToDictionary(m => m.Id);
        var firearmById = _data.Firearms.ToDictionary(f => f.Id);

        foreach (var mag in _data.Magazines)
        {
            mag.Modifications = _data.MagazineModifications.Where(m => m.MagazineId == mag.Id).ToList();
            foreach (var m in mag.Modifications) m.Magazine = mag;

            mag.Malfunctions = _data.MagazineMalfunctions.Where(m => m.MagazineId == mag.Id).ToList();
            foreach (var m in mag.Malfunctions)
            {
                m.Magazine = mag;
                m.Ammo = m.AmmoId.HasValue ? ammoById.GetValueOrDefault(m.AmmoId.Value) : null;
            }
        }

        foreach (var s in _data.RoundSessions)
        {
            s.Firearm = firearmById.GetValueOrDefault(s.FirearmId)!;
            s.Magazine = s.MagazineId.HasValue ? magazineById.GetValueOrDefault(s.MagazineId.Value) : null;
            s.Ammo = s.AmmoId.HasValue ? ammoById.GetValueOrDefault(s.AmmoId.Value) : null;
        }

        foreach (var m in _data.FirearmMalfunctions)
        {
            m.Firearm = firearmById.GetValueOrDefault(m.FirearmId)!;
            m.Ammo = m.AmmoId.HasValue ? ammoById.GetValueOrDefault(m.AmmoId.Value) : null;
        }

        foreach (var stage in _data.HitFactorStages)
        {
            var session = _data.HitFactorSessions.FirstOrDefault(s => s.Id == stage.HitFactorSessionId);
            if (session != null) stage.Session = session;
        }

        foreach (var session in _data.HitFactorSessions)
        {
            session.Firearm = session.FirearmId.HasValue ? firearmById.GetValueOrDefault(session.FirearmId.Value) : null;
            session.Stages = _data.HitFactorStages.Where(s => s.HitFactorSessionId == session.Id).ToList();
        }

        foreach (var log in _data.MaintenanceLogs)
        {
            log.Firearm = firearmById.GetValueOrDefault(log.FirearmId)!;
            log.CleanedParts = _data.MaintenanceLogParts.Where(p => p.MaintenanceLogId == log.Id).ToList();
            foreach (var p in log.CleanedParts) p.MaintenanceLog = log;
        }

        foreach (var part in _data.MaintenanceParts)
            part.Firearm = firearmById.GetValueOrDefault(part.FirearmId)!;

        foreach (var sched in _data.MaintenanceSchedules)
            sched.Firearm = firearmById.GetValueOrDefault(sched.FirearmId)!;

        foreach (var mod in _data.Modifications)
            mod.Firearm = firearmById.GetValueOrDefault(mod.FirearmId)!;

        foreach (var target in _data.DrillTargets)
        {
            var drill = _data.Drills.FirstOrDefault(d => d.Id == target.DrillId);
            if (drill != null) target.Drill = drill;
        }

        foreach (var session in _data.DrillSessions)
        {
            session.Drill = _data.Drills.First(d => d.Id == session.DrillId);
            session.Firearm = session.FirearmId.HasValue ? firearmById.GetValueOrDefault(session.FirearmId.Value) : null;
        }

        foreach (var drill in _data.Drills)
        {
            drill.Targets = _data.DrillTargets.Where(t => t.DrillId == drill.Id).OrderBy(t => t.OrderIndex).ToList();
            drill.Sessions = _data.DrillSessions.Where(s => s.DrillId == drill.Id).ToList();
        }

        foreach (var firearm in _data.Firearms)
        {
            firearm.Modifications = _data.Modifications.Where(m => m.FirearmId == firearm.Id).ToList();
            firearm.Magazines = _data.Magazines.Where(m => m.FirearmId == firearm.Id).ToList();
            foreach (var mag in firearm.Magazines) mag.Firearm = firearm;
            firearm.Sessions = _data.RoundSessions.Where(s => s.FirearmId == firearm.Id).ToList();
            firearm.Malfunctions = _data.FirearmMalfunctions.Where(m => m.FirearmId == firearm.Id).ToList();
            firearm.Schedule = _data.MaintenanceSchedules.FirstOrDefault(s => s.FirearmId == firearm.Id);
            firearm.Parts = _data.MaintenanceParts.Where(p => p.FirearmId == firearm.Id).ToList();
            firearm.MaintenanceLogs = _data.MaintenanceLogs.Where(l => l.FirearmId == firearm.Id).ToList();
        }
    }

    // Call after any Add/Update/Remove on the flat lists so navigation properties
    // stay consistent for subsequent in-memory reads, without every service method
    // needing to remember to do it itself.
    public async Task SaveAsync()
    {
        Hydrate();

        await _lock.WaitAsync();
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(_data, JsonOptions);
            var tmpPath = _filePath + ".tmp";
            await File.WriteAllTextAsync(tmpPath, json);
            File.Move(tmpPath, _filePath, overwrite: true);
        }
        finally
        {
            _lock.Release();
        }
    }
}
