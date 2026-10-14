namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalVisitorScheduleChecks
{
    public static void SeasonalVisitorScheduleStartsInCreated()
    {
        var item = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule("code-318", "name-537");
        Check.Equal(item.Status, SeasonalVisitorScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalVisitorScheduleMovesToPublished()
    {
        var item = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule("code-318", "name-537");
        try
        {
            item.MoveTo(SeasonalVisitorScheduleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalVisitorScheduleStatus.Published, "status");
    }

    public static void SeasonalVisitorScheduleRejectsMovesFromRetired()
    {
        var item = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule("code-318", "name-537");
        item.Status = SeasonalVisitorScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(SeasonalVisitorScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        SeasonalVisitorScheduleStartsInCreated();
        SeasonalVisitorScheduleMovesToPublished();
        SeasonalVisitorScheduleRejectsMovesFromRetired();
    }
}
