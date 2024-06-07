namespace Lagerhotell.Services.State;
public class AppState
{
    public bool IsUserLoggedIn { get; private set; }
    public event Action OnChange;
    public void NotifyStateChanged() => OnChange?.Invoke();

    public void UserLoggedIn()
    {
        IsUserLoggedIn = true;
        NotifyStateChanged();
    }
}
