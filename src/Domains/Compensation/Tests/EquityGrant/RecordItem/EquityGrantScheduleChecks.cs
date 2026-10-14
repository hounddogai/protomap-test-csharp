namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantScheduleChecks
{
    public static void EquityGrantScheduleStartsInIdle()
    {
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
        Check.Equal(item.Status, EquityGrantScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantScheduleMovesToDraining()
    {
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
        try
        {
            item.MoveTo(EquityGrantScheduleStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantScheduleStatus.Draining, "status");
    }

    public static void EquityGrantScheduleRejectsMovesFromStopped()
    {
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
        item.Status = EquityGrantScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(EquityGrantScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EquityGrantScheduleStartsInIdle();
        EquityGrantScheduleMovesToDraining();
        EquityGrantScheduleRejectsMovesFromStopped();
    }
}
