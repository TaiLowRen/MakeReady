using MakeReady.Models;

namespace MakeReady.Services;

// Singleton that holds pending maintenance alerts so any page can read them.
// Populated on app startup and refreshed when the Maintenance page loads.
public class AlertService
{
    private List<MaintenanceAlert> _alerts = new();

    public IReadOnlyList<MaintenanceAlert> ActiveAlerts => _alerts;
    public int OverdueCount => _alerts.Count(a => a.Status == MaintenanceStatusLevel.Overdue);
    public int DueSoonCount => _alerts.Count(a => a.Status == MaintenanceStatusLevel.DueSoon);
    public bool HasAlerts    => _alerts.Count > 0;

    public event Action? OnAlertsChanged;

    public void SetAlerts(List<MaintenanceAlert> alerts)
    {
        _alerts = alerts;
        OnAlertsChanged?.Invoke();
    }

    public void Clear() => SetAlerts(new());
}
