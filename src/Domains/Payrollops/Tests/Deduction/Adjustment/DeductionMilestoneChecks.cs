namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionMilestoneChecks
{
    public static void DeductionMilestoneStartsInIdle()
    {
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
        Check.Equal(item.Status, DeductionMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionMilestoneMovesToDraining()
    {
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
        try
        {
            item.MoveTo(DeductionMilestoneStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionMilestoneStatus.Draining, "status");
    }

    public static void DeductionMilestoneRejectsMovesFromStopped()
    {
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
        item.Status = DeductionMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionMilestoneStartsInIdle();
        DeductionMilestoneMovesToDraining();
        DeductionMilestoneRejectsMovesFromStopped();
    }
}
