namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictDeductionMilestoneChecks
{
    public static void StrictDeductionMilestoneStartsInProposed()
    {
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("code-966", "name-193");
        Check.Equal(item.Status, StrictDeductionMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictDeductionMilestoneMovesToRejected()
    {
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("code-966", "name-193");
        try
        {
            item.MoveTo(StrictDeductionMilestoneStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictDeductionMilestoneStatus.Rejected, "status");
    }

    public static void StrictDeductionMilestoneRejectsMovesFromWithdrawn()
    {
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("code-966", "name-193");
        item.Status = StrictDeductionMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictDeductionMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictDeductionMilestoneStartsInProposed();
        StrictDeductionMilestoneMovesToRejected();
        StrictDeductionMilestoneRejectsMovesFromWithdrawn();
    }
}
