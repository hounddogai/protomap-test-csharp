namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RegionalDeductionScheduleChecks
{
    public static void RegionalDeductionScheduleStartsInProposed()
    {
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("code-650", "label-800");
        Check.Equal(item.Status, RegionalDeductionScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeductionScheduleMovesToRejected()
    {
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("code-650", "label-800");
        try
        {
            item.MoveTo(RegionalDeductionScheduleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeductionScheduleStatus.Rejected, "status");
    }

    public static void RegionalDeductionScheduleRejectsMovesFromWithdrawn()
    {
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("code-650", "label-800");
        item.Status = RegionalDeductionScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalDeductionScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalDeductionScheduleStartsInProposed();
        RegionalDeductionScheduleMovesToRejected();
        RegionalDeductionScheduleRejectsMovesFromWithdrawn();
    }
}
