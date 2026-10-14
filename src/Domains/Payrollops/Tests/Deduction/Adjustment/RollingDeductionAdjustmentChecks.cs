namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingDeductionAdjustmentChecks
{
    public static void RollingDeductionAdjustmentStartsInNew()
    {
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("code-194", "name-82");
        Check.Equal(item.Status, RollingDeductionAdjustmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingDeductionAdjustmentMovesToRunning()
    {
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("code-194", "name-82");
        try
        {
            item.MoveTo(RollingDeductionAdjustmentStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingDeductionAdjustmentStatus.Running, "status");
    }

    public static void RollingDeductionAdjustmentRejectsMovesFromFailed()
    {
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("code-194", "name-82");
        item.Status = RollingDeductionAdjustmentStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingDeductionAdjustmentStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingDeductionAdjustmentStartsInNew();
        RollingDeductionAdjustmentMovesToRunning();
        RollingDeductionAdjustmentRejectsMovesFromFailed();
    }
}
