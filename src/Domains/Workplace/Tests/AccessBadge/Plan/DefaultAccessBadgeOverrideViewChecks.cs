namespace Acme.Workforce.Workplace.Tests.AccessBadge.Plan;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultAccessBadgeOverrideViewChecks
{
    public static void DefaultAccessBadgeOverrideViewShowsTheStatusInCapitals()
    {
        var item = DefaultAccessBadgeOverride.NewDefaultAccessBadgeOverride("code-529", "title-767");
        var view = DefaultAccessBadgeOverrideView.ToDefaultAccessBadgeOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAccessBadgeOverrideView.ToDefaultAccessBadgeOverrideViews(new List<DefaultAccessBadgeOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeOverrideViewShowsTheStatusInCapitals();
    }
}
