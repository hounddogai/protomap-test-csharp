namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionAllocationChecks
{
    public static void DeductionAllocationStartsInNew()
    {
        var item = DeductionAllocation.NewDeductionAllocation("code-631", "title-899");
        Check.Equal(item.Status, DeductionAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionAllocationMovesToQueued()
    {
        var item = DeductionAllocation.NewDeductionAllocation("code-631", "title-899");
        try
        {
            item.MoveTo(DeductionAllocationStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionAllocationStatus.Queued, "status");
    }

    public static void DeductionAllocationRejectsMovesFromFailed()
    {
        var item = DeductionAllocation.NewDeductionAllocation("code-631", "title-899");
        item.Status = DeductionAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(DeductionAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        DeductionAllocationStartsInNew();
        DeductionAllocationMovesToQueued();
        DeductionAllocationRejectsMovesFromFailed();
    }
}
