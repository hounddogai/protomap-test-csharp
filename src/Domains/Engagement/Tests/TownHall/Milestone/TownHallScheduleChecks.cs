namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallScheduleChecks
{
    public static void TownHallScheduleStartsInRequested()
    {
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
        Check.Equal(item.Status, TownHallScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallScheduleMovesToExpired()
    {
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
        try
        {
            item.MoveTo(TownHallScheduleStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallScheduleStatus.Expired, "status");
    }

    public static void TownHallScheduleRejectsMovesFromExpired()
    {
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
        item.Status = TownHallScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(TownHallScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TownHallScheduleStartsInRequested();
        TownHallScheduleMovesToExpired();
        TownHallScheduleRejectsMovesFromExpired();
    }
}
