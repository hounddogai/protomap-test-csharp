namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskSummaryViewChecks
{
    public static void RegionalDeskSummaryViewShowsTheStatusInCapitals()
    {
        var item = RegionalDeskSummary.NewRegionalDeskSummary("code-286", "title-923");
        var view = RegionalDeskSummaryView.ToRegionalDeskSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalDeskSummaryView.ToRegionalDeskSummaryViews(new List<RegionalDeskSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalDeskSummaryViewShowsTheStatusInCapitals();
    }
}
