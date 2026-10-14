namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallScheduleViewChecks
{
    public static void LegacyTownHallScheduleViewShowsTheStatusInCapitals()
    {
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("code-55", "title-750");
        var view = LegacyTownHallScheduleView.ToLegacyTownHallScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTownHallScheduleView.ToLegacyTownHallScheduleViews(new List<LegacyTownHallSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTownHallScheduleViewShowsTheStatusInCapitals();
    }
}
