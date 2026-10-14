namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVisitorOverrideViewChecks
{
    public static void AdaptiveVisitorOverrideViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveVisitorOverride.NewAdaptiveVisitorOverride("code-496", "name-747");
        var view = AdaptiveVisitorOverrideView.ToAdaptiveVisitorOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveVisitorOverrideView.ToAdaptiveVisitorOverrideViews(new List<AdaptiveVisitorOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveVisitorOverrideViewShowsTheStatusInCapitals();
    }
}
