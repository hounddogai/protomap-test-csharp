namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class DefaultAccessBadgeRollupViewChecks
{
    public static void DefaultAccessBadgeRollupViewShowsTheStatusInCapitals()
    {
        var item = DefaultAccessBadgeRollup.NewDefaultAccessBadgeRollup("code-16", "label-466");
        var view = DefaultAccessBadgeRollupView.ToDefaultAccessBadgeRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAccessBadgeRollupView.ToDefaultAccessBadgeRollupViews(new List<DefaultAccessBadgeRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAccessBadgeRollupViewShowsTheStatusInCapitals();
    }
}
