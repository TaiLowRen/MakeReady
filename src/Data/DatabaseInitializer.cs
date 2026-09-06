using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MakeReady.Data;

// Wraps AppDbContext creation so the first call into native SQLite (Batteries_V2.Init,
// EnsureCreated, schema updates) happens in the background instead of blocking app
// launch. On iOS that native call has been observed to take long enough to trip the
// OS's scene-create launch watchdog; deferring it past the first render avoids that
// regardless of why it's slow.
public class DatabaseInitializer : IDbContextFactory<AppDbContext>
{
    readonly DbContextOptions<AppDbContext> _options;
    readonly Task _ready;

    public DatabaseInitializer(DbContextOptions<AppDbContext> options)
    {
        _options = options;
        _ready = Task.Run(InitializeAsync);
    }

    public AppDbContext CreateDbContext()
    {
        _ready.GetAwaiter().GetResult();
        return new AppDbContext(_options);
    }

    public async Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        await _ready;
        return new AppDbContext(_options);
    }

    async Task InitializeAsync()
    {
        await Task.Yield();

        SQLitePCL.Batteries_V2.Init();

        using var db = new AppDbContext(_options);

        // SQLite's WAL journal mode (which EF Core's SqliteDatabaseCreator enables by
        // default when creating a new database) has been unreliable on iOS's sandboxed
        // filesystem. Force the traditional rollback journal before EnsureCreated()
        // gets a chance to switch to WAL.
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "PRAGMA journal_mode=DELETE;";
            cmd.ExecuteNonQuery();
        }

        db.Database.EnsureCreated();
        ApplySchemaUpdates(db);
    }

    // Additive-only schema updates for existing databases.
    // EnsureCreated() only runs once (on first install), so new tables added in later
    // versions must be created here with IF NOT EXISTS guards.
    static void ApplySchemaUpdates(AppDbContext db)
    {
        var conn = db.Database.GetDbConnection();

        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS MagazineModifications (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                MagazineId INTEGER NOT NULL REFERENCES Magazines(Id) ON DELETE CASCADE,
                Name      TEXT NOT NULL,
                Type      INTEGER NOT NULL,
                Notes     TEXT,
                AddedAt   TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS MagazineMalfunctions (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                MagazineId INTEGER NOT NULL REFERENCES Magazines(Id) ON DELETE CASCADE,
                Type       INTEGER NOT NULL,
                Date       TEXT NOT NULL,
                Notes      TEXT
            );
            CREATE TABLE IF NOT EXISTS FirearmMalfunctions (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirearmId INTEGER NOT NULL REFERENCES Firearms(Id) ON DELETE CASCADE,
                Type      INTEGER NOT NULL,
                Date      TEXT NOT NULL,
                Notes     TEXT
            );
            CREATE TABLE IF NOT EXISTS HitFactorSessions (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirearmId INTEGER REFERENCES Firearms(Id) ON DELETE SET NULL,
                Name      TEXT NOT NULL,
                Date      TEXT NOT NULL,
                Location  TEXT,
                Notes     TEXT
            );
            CREATE TABLE IF NOT EXISTS HitFactorStages (
                Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                HitFactorSessionId  INTEGER NOT NULL REFERENCES HitFactorSessions(Id) ON DELETE CASCADE,
                StageName           TEXT,
                AHits               INTEGER NOT NULL DEFAULT 0,
                CHits               INTEGER NOT NULL DEFAULT 0,
                DHits               INTEGER NOT NULL DEFAULT 0,
                Misses              INTEGER NOT NULL DEFAULT 0,
                NoShoots            INTEGER NOT NULL DEFAULT 0,
                Procedurals         INTEGER NOT NULL DEFAULT 0,
                Time                REAL NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS MaintenanceSchedules (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirearmId INTEGER NOT NULL UNIQUE REFERENCES Firearms(Id) ON DELETE CASCADE,
                IntervalDays         INTEGER NOT NULL DEFAULT 90,
                NotificationsEnabled INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS MaintenanceParts (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirearmId INTEGER NOT NULL REFERENCES Firearms(Id) ON DELETE CASCADE,
                Name      TEXT NOT NULL,
                IntervalDays         INTEGER,
                NotificationsEnabled INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS MaintenanceLogs (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                FirearmId INTEGER NOT NULL REFERENCES Firearms(Id) ON DELETE CASCADE,
                Date      TEXT NOT NULL,
                Notes     TEXT
            );
            CREATE TABLE IF NOT EXISTS MaintenanceLogParts (
                Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                MaintenanceLogId   INTEGER NOT NULL REFERENCES MaintenanceLogs(Id) ON DELETE CASCADE,
                PartName           TEXT NOT NULL,
                MaintenancePartId  INTEGER REFERENCES MaintenanceParts(Id) ON DELETE SET NULL
            );";
        cmd.ExecuteNonQuery();

        // New table: Ammos
        cmd.CommandText += @"
            CREATE TABLE IF NOT EXISTS Ammos (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Brand   TEXT NOT NULL,
                Caliber TEXT,
                Grain   INTEGER NOT NULL DEFAULT 0,
                Type    INTEGER NOT NULL DEFAULT 0,
                Notes   TEXT
            );";
        cmd.ExecuteNonQuery();

        // ALTER TABLE is not idempotent in SQLite -- ignore if column already exists
        var alters = new[]
        {
            "ALTER TABLE Firearms ADD COLUMN Category INTEGER NOT NULL DEFAULT 0;",
            "ALTER TABLE RoundSessions ADD COLUMN AmmoId INTEGER REFERENCES Ammos(Id) ON DELETE SET NULL;",
            "ALTER TABLE FirearmMalfunctions ADD COLUMN AmmoId INTEGER REFERENCES Ammos(Id) ON DELETE SET NULL;",
            "ALTER TABLE MagazineMalfunctions ADD COLUMN AmmoId INTEGER REFERENCES Ammos(Id) ON DELETE SET NULL;",
            "ALTER TABLE MaintenanceParts ADD COLUMN Brand TEXT;"
        };
        foreach (var sql in alters)
        {
            try
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = sql;
                alter.ExecuteNonQuery();
            }
            catch { /* column already exists */ }
        }
    }
}
