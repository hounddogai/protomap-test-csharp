namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceMilestoneViewChecks
{
    public static void BalanceMilestoneViewShowsTheStatusInCapitals()
    {
        var item = BalanceMilestone.NewBalanceMilestone("code-126", "label-713");
        var view = BalanceMilestoneView.ToBalanceMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceMilestoneView.ToBalanceMilestoneViews(new List<BalanceMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceMilestoneViewShowsTheStatusInCapitals();
    }
}
