namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientAdjustmentScheduleChecks
{
    public static void LenientAdjustmentScheduleStartsInPlanned()
    {
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("code-832", "name-223");
        Check.Equal(item.Status, LenientAdjustmentScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAdjustmentScheduleMovesToScheduled()
    {
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("code-832", "name-223");
        try
        {
            item.MoveTo(LenientAdjustmentScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAdjustmentScheduleStatus.Scheduled, "status");
    }

    public static void LenientAdjustmentScheduleRejectsMovesFromCancelled()
    {
        var item = LenientAdjustmentSchedule.NewLenientAdjustmentSchedule("code-832", "name-223");
        item.Status = LenientAdjustmentScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientAdjustmentScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientAdjustmentScheduleStartsInPlanned();
        LenientAdjustmentScheduleMovesToScheduled();
        LenientAdjustmentScheduleRejectsMovesFromCancelled();
    }
}
