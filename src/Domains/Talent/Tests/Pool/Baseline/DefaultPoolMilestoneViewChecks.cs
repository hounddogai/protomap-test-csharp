namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolMilestoneViewChecks
{
    public static void DefaultPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = DefaultPoolMilestone.NewDefaultPoolMilestone("code-919", "name-870");
        var view = DefaultPoolMilestoneView.ToDefaultPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPoolMilestoneView.ToDefaultPoolMilestoneViews(new List<DefaultPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
