namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class StrictPoolScheduleChecks
{
    public static void StrictPoolScheduleStartsInNew()
    {
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
        Check.Equal(item.Status, StrictPoolScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPoolScheduleMovesToFailed()
    {
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
        try
        {
            item.MoveTo(StrictPoolScheduleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPoolScheduleStatus.Failed, "status");
    }

    public static void StrictPoolScheduleRejectsMovesFromFailed()
    {
        var item = StrictPoolSchedule.NewStrictPoolSchedule("code-70", "name-626");
        item.Status = StrictPoolScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictPoolScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictPoolScheduleStartsInNew();
        StrictPoolScheduleMovesToFailed();
        StrictPoolScheduleRejectsMovesFromFailed();
    }
}
