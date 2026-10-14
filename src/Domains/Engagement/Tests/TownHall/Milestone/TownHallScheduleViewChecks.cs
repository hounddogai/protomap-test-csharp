namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallScheduleViewChecks
{
    public static void TownHallScheduleViewShowsTheStatusInCapitals()
    {
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
        var view = TownHallScheduleView.ToTownHallScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallScheduleView.ToTownHallScheduleViews(new List<TownHallSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallScheduleViewShowsTheStatusInCapitals();
    }
}
