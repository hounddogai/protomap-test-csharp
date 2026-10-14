namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientRefreshProfileViewChecks
{
    public static void LenientRefreshProfileViewShowsTheStatusInCapitals()
    {
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
        var view = LenientRefreshProfileView.ToLenientRefreshProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRefreshProfileView.ToLenientRefreshProfileViews(new List<LenientRefreshProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRefreshProfileViewShowsTheStatusInCapitals();
    }
}
