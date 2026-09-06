namespace MakeReady.Models;

public enum MaintenanceStatusLevel { OK, DueSoon, Overdue }

public record MaintenanceAlert(
    int FirearmId,
    string FirearmName,
    FirearmCategory Category,
    MaintenanceStatusLevel Status,
    string Message,
    DateTime? LastCleaned,
    int? DaysUntilDue,
    int SessionsSinceLastClean
);

public record PartAlert(
    int FirearmId,
    string FirearmName,
    int PartId,
    string PartName,
    MaintenanceStatusLevel Status,
    string Message,
    DateTime? LastCleaned,
    int? DaysUntilDue
);
