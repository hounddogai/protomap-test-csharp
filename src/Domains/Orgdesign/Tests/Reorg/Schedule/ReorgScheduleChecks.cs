namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgScheduleChecks
{
    public static void ReorgScheduleStartsInCreated()
    {
        var item = ReorgSchedule.NewReorgSchedule("code-407", "label-415");
        Check.Equal(item.Status, ReorgScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgScheduleMovesToPublished()
    {
        var item = ReorgSchedule.NewReorgSchedule("code-407", "label-415");
        try
        {
            item.MoveTo(ReorgScheduleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgScheduleStatus.Published, "status");
    }

    public static void ReorgScheduleRejectsMovesFromRetired()
    {
        var item = ReorgSchedule.NewReorgSchedule("code-407", "label-415");
        item.Status = ReorgScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(ReorgScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReorgScheduleStartsInCreated();
        ReorgScheduleMovesToPublished();
        ReorgScheduleRejectsMovesFromRetired();
    }
}
