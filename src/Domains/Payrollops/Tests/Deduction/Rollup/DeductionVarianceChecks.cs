namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionVarianceChecks
{
    public static void DeductionVarianceStartsInIdle()
    {
        var item = DeductionVariance.NewDeductionVariance("code-902", "title-344");
        Check.Equal(item.Status, DeductionVarianceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionVarianceMovesToStopped()
    {
        var item = DeductionVariance.NewDeductionVariance("code-902", "title-344");
        try
        {
            item.MoveTo(DeductionVarianceStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionVarianceStatus.Stopped, "status");
    }

    public static void DeductionVarianceRejectsMovesFromStopped()
    {
        var item = DeductionVariance.NewDeductionVariance("code-902", "title-344");
        item.Status = DeductionVarianceStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionVarianceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionVarianceStartsInIdle();
        DeductionVarianceMovesToStopped();
        DeductionVarianceRejectsMovesFromStopped();
    }
}
