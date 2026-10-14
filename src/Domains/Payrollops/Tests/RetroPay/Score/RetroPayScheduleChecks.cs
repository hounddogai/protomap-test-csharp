namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RetroPayScheduleChecks
{
    public static void RetroPayScheduleStartsInPlanned()
    {
        var item = RetroPaySchedule.NewRetroPaySchedule("code-778", "label-940");
        Check.Equal(item.Status, RetroPayScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayScheduleMovesToCancelled()
    {
        var item = RetroPaySchedule.NewRetroPaySchedule("code-778", "label-940");
        try
        {
            item.MoveTo(RetroPayScheduleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayScheduleStatus.Cancelled, "status");
    }

    public static void RetroPayScheduleRejectsMovesFromCancelled()
    {
        var item = RetroPaySchedule.NewRetroPaySchedule("code-778", "label-940");
        item.Status = RetroPayScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayScheduleStartsInPlanned();
        RetroPayScheduleMovesToCancelled();
        RetroPayScheduleRejectsMovesFromCancelled();
    }
}
