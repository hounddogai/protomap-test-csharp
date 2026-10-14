namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeSummaryViewChecks
{
    public static void AccessBadgeSummaryViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeSummary.NewAccessBadgeSummary("code-592", "name-680");
        var view = AccessBadgeSummaryView.ToAccessBadgeSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeSummaryView.ToAccessBadgeSummaryViews(new List<AccessBadgeSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeSummaryViewShowsTheStatusInCapitals();
    }
}
