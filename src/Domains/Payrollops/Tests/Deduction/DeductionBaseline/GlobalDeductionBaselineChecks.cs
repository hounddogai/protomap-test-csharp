namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class GlobalDeductionBaselineChecks
{
    public static void GlobalDeductionBaselineStartsInPlanned()
    {
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("code-976", "label-681");
        Check.Equal(item.Status, GlobalDeductionBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeductionBaselineMovesToCompleted()
    {
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("code-976", "label-681");
        try
        {
            item.MoveTo(GlobalDeductionBaselineStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeductionBaselineStatus.Completed, "status");
    }

    public static void GlobalDeductionBaselineRejectsMovesFromCancelled()
    {
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("code-976", "label-681");
        item.Status = GlobalDeductionBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalDeductionBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalDeductionBaselineStartsInPlanned();
        GlobalDeductionBaselineMovesToCompleted();
        GlobalDeductionBaselineRejectsMovesFromCancelled();
    }
}
