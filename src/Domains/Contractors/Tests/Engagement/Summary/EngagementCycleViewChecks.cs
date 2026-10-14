namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EngagementCycleViewChecks
{
    public static void EngagementCycleViewShowsTheStatusInCapitals()
    {
        var item = EngagementCycle.NewEngagementCycle("code-510", "label-95");
        var view = EngagementCycleView.ToEngagementCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementCycleView.ToEngagementCycleViews(new List<EngagementCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementCycleViewShowsTheStatusInCapitals();
    }
}
