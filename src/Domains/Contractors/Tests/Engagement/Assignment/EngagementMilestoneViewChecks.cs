namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementMilestoneViewChecks
{
    public static void EngagementMilestoneViewShowsTheStatusInCapitals()
    {
        var item = EngagementMilestone.NewEngagementMilestone("code-394", "label-948");
        var view = EngagementMilestoneView.ToEngagementMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementMilestoneView.ToEngagementMilestoneViews(new List<EngagementMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementMilestoneViewShowsTheStatusInCapitals();
    }
}
