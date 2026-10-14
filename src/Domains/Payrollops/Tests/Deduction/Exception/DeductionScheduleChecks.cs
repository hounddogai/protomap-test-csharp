namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionScheduleChecks
{
    public static void DeductionScheduleStartsInProposed()
    {
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
        Check.Equal(item.Status, DeductionScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionScheduleMovesToApproved()
    {
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
        try
        {
            item.MoveTo(DeductionScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionScheduleStatus.Approved, "status");
    }

    public static void DeductionScheduleRejectsMovesFromWithdrawn()
    {
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
        item.Status = DeductionScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionScheduleStartsInProposed();
        DeductionScheduleMovesToApproved();
        DeductionScheduleRejectsMovesFromWithdrawn();
    }
}
