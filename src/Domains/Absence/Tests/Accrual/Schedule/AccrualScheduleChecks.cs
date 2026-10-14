namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AccrualScheduleChecks
{
    public static void AccrualScheduleStartsInIdle()
    {
        var item = AccrualSchedule.NewAccrualSchedule("code-847", "label-38");
        Check.Equal(item.Status, AccrualScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualScheduleMovesToDraining()
    {
        var item = AccrualSchedule.NewAccrualSchedule("code-847", "label-38");
        try
        {
            item.MoveTo(AccrualScheduleStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualScheduleStatus.Draining, "status");
    }

    public static void AccrualScheduleRejectsMovesFromStopped()
    {
        var item = AccrualSchedule.NewAccrualSchedule("code-847", "label-38");
        item.Status = AccrualScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccrualScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccrualScheduleStartsInIdle();
        AccrualScheduleMovesToDraining();
        AccrualScheduleRejectsMovesFromStopped();
    }
}
