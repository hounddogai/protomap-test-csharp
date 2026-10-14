namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshDigestViewChecks
{
    public static void RefreshDigestViewShowsTheStatusInCapitals()
    {
        var item = RefreshDigest.NewRefreshDigest("code-709", "title-906");
        var view = RefreshDigestView.ToRefreshDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshDigestView.ToRefreshDigestViews(new List<RefreshDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshDigestViewShowsTheStatusInCapitals();
    }
}
