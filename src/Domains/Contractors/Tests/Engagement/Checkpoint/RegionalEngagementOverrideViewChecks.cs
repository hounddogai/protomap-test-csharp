namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalEngagementOverrideViewChecks
{
    public static void RegionalEngagementOverrideViewShowsTheStatusInCapitals()
    {
        var item = RegionalEngagementOverride.NewRegionalEngagementOverride("code-9", "name-166");
        var view = RegionalEngagementOverrideView.ToRegionalEngagementOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalEngagementOverrideView.ToRegionalEngagementOverrideViews(new List<RegionalEngagementOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalEngagementOverrideViewShowsTheStatusInCapitals();
    }
}
