namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyTownHallScheduleChecks
{
    public static void LegacyTownHallScheduleStartsInOpen()
    {
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("code-55", "title-750");
        Check.Equal(item.Status, LegacyTownHallScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallScheduleMovesToResolved()
    {
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("code-55", "title-750");
        try
        {
            item.MoveTo(LegacyTownHallScheduleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallScheduleStatus.Resolved, "status");
    }

    public static void LegacyTownHallScheduleRejectsMovesFromArchived()
    {
        var item = LegacyTownHallSchedule.NewLegacyTownHallSchedule("code-55", "title-750");
        item.Status = LegacyTownHallScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyTownHallScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallScheduleStartsInOpen();
        LegacyTownHallScheduleMovesToResolved();
        LegacyTownHallScheduleRejectsMovesFromArchived();
    }
}
