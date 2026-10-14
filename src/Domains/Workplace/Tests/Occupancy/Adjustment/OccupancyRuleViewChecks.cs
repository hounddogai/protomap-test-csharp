namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyRuleViewChecks
{
    public static void OccupancyRuleViewShowsTheStatusInCapitals()
    {
        var item = OccupancyRule.NewOccupancyRule("code-760", "title-177");
        var view = OccupancyRuleView.ToOccupancyRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyRuleView.ToOccupancyRuleViews(new List<OccupancyRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyRuleViewShowsTheStatusInCapitals();
    }
}
