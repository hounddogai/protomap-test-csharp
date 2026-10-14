namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneViewChecks
{
    public static void GlobalNominationMilestoneViewShowsTheStatusInCapitals()
    {
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
        var view = GlobalNominationMilestoneView.ToGlobalNominationMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalNominationMilestoneView.ToGlobalNominationMilestoneViews(new List<GlobalNominationMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalNominationMilestoneViewShowsTheStatusInCapitals();
    }
}
