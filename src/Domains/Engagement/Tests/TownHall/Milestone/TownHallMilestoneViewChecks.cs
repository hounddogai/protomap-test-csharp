namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMilestoneViewChecks
{
    public static void TownHallMilestoneViewShowsTheStatusInCapitals()
    {
        var item = TownHallMilestone.NewTownHallMilestone("code-6", "label-286");
        var view = TownHallMilestoneView.ToTownHallMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallMilestoneView.ToTownHallMilestoneViews(new List<TownHallMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallMilestoneViewShowsTheStatusInCapitals();
    }
}
