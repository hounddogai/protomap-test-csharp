namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementTierViewChecks
{
    public static void EngagementTierViewShowsTheStatusInCapitals()
    {
        var item = EngagementTier.NewEngagementTier("code-516", "label-748");
        var view = EngagementTierView.ToEngagementTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementTierView.ToEngagementTierViews(new List<EngagementTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementTierViewShowsTheStatusInCapitals();
    }
}
