namespace Acme.Workforce.Assets.Tests.Refresh.RefreshReview;

using Acme.Workforce.Assets.Refresh.RefreshReview;

/// <summary>Checks of refresh review code, run by the project's runner.</summary>
public static class LegacyRefreshProfileViewChecks
{
    public static void LegacyRefreshProfileViewShowsTheStatusInCapitals()
    {
        var item = LegacyRefreshProfile.NewLegacyRefreshProfile("code-50", "label-555");
        var view = LegacyRefreshProfileView.ToLegacyRefreshProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyRefreshProfileView.ToLegacyRefreshProfileViews(new List<LegacyRefreshProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyRefreshProfileViewShowsTheStatusInCapitals();
    }
}
