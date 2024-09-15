namespace Lagerhotell.Services.State;
public class AppState
{
    public bool IsUserLoggedIn { get; private set; }
    public event Action OnChange;
    public User UserBeforeEmailVerified { get; set; }
    public CompanyUser CompanyUserBeforeVerified { get; set; }
    public void NotifyStateChanged() => OnChange?.Invoke();

    public void UserLoggedIn()
    {
        IsUserLoggedIn = true;
        NotifyStateChanged();
    }

    public void UserLoggedOut()
    {
        IsUserLoggedIn = false;
        NotifyStateChanged();
    }

    public void SetUserBeforeEmailVerified(User user)
    {
        UserBeforeEmailVerified = user;
        NotifyStateChanged();
    }

    public void SetCompanyUserBeforeEmailVerified(CompanyUser companyUser)
    {
        CompanyUserBeforeVerified = companyUser;
        NotifyStateChanged();
    }
}
