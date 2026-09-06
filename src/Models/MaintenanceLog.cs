namespace MakeReady.Models;

public class MaintenanceLog
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    public Firearm Firearm { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public List<MaintenanceLogPart> CleanedParts { get; set; } = new();
}

public class MaintenanceLogPart
{
    public int Id { get; set; }
    public int MaintenanceLogId { get; set; }
    public MaintenanceLog MaintenanceLog { get; set; } = null!;
    // Stored by name so history survives part deletion
    public string PartName { get; set; } = "";
    public int? MaintenancePartId { get; set; }
}
