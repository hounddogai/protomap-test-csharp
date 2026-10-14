namespace Acme.Workforce.Assets.Tests.Refresh.Tier;

using Acme.Workforce.Assets.Refresh.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RefreshProfileViewChecks
{
    public static void RefreshProfileViewShowsTheStatusInCapitals()
    {
        var item = RefreshProfile.NewRefreshProfile("code-817", "title-285");
        var view = RefreshProfileView.ToRefreshProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshProfileView.ToRefreshProfileViews(new List<RefreshProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshProfileViewShowsTheStatusInCapitals();
    }
}
