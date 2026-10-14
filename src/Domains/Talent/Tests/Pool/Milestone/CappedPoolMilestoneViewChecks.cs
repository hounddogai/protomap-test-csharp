namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedPoolMilestoneViewChecks
{
    public static void CappedPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = CappedPoolMilestone.NewCappedPoolMilestone("code-43", "name-364");
        var view = CappedPoolMilestoneView.ToCappedPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPoolMilestoneView.ToCappedPoolMilestoneViews(new List<CappedPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
