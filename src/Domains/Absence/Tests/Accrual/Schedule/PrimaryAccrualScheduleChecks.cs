namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryAccrualScheduleChecks
{
    public static void PrimaryAccrualScheduleStartsInPlanned()
    {
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("code-131", "label-204");
        Check.Equal(item.Status, PrimaryAccrualScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAccrualScheduleMovesToScheduled()
    {
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("code-131", "label-204");
        try
        {
            item.MoveTo(PrimaryAccrualScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAccrualScheduleStatus.Scheduled, "status");
    }

    public static void PrimaryAccrualScheduleRejectsMovesFromCancelled()
    {
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("code-131", "label-204");
        item.Status = PrimaryAccrualScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryAccrualScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryAccrualScheduleStartsInPlanned();
        PrimaryAccrualScheduleMovesToScheduled();
        PrimaryAccrualScheduleRejectsMovesFromCancelled();
    }
}
