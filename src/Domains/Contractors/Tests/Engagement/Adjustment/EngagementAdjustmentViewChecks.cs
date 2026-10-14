namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementAdjustmentViewChecks
{
    public static void EngagementAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = EngagementAdjustment.NewEngagementAdjustment("code-374", "title-852");
        var view = EngagementAdjustmentView.ToEngagementAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementAdjustmentView.ToEngagementAdjustmentViews(new List<EngagementAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementAdjustmentViewShowsTheStatusInCapitals();
    }
}
