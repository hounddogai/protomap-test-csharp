namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class GlobalAccessBadgeRollupViewChecks
{
    public static void GlobalAccessBadgeRollupViewShowsTheStatusInCapitals()
    {
        var item = GlobalAccessBadgeRollup.NewGlobalAccessBadgeRollup("code-637", "name-552");
        var view = GlobalAccessBadgeRollupView.ToGlobalAccessBadgeRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAccessBadgeRollupView.ToGlobalAccessBadgeRollupViews(new List<GlobalAccessBadgeRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAccessBadgeRollupViewShowsTheStatusInCapitals();
    }
}
