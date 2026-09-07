namespace MakeReady.Services;

// Singleton shared between each page's hamburger button and the NavMenu drawer
// component (which lives once, in MainLayout), so opening the drawer from any
// page's header can toggle the one shared drawer instance.
public class NavDrawerState
{
    public bool IsOpen { get; private set; }

    public event Action? Changed;

    public void Open()
    {
        IsOpen = true;
        Changed?.Invoke();
    }

    public void Close()
    {
        IsOpen = false;
        Changed?.Invoke();
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;
        Changed?.Invoke();
    }
}
