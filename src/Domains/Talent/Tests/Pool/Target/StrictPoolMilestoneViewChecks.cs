namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictPoolMilestoneViewChecks
{
    public static void StrictPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
        var view = StrictPoolMilestoneView.ToStrictPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPoolMilestoneView.ToStrictPoolMilestoneViews(new List<StrictPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
