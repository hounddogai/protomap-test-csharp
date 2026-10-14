namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionRollupChecks
{
    public static void DeductionRollupStartsInDraft()
    {
        var item = DeductionRollup.NewDeductionRollup("code-190", "name-688");
        Check.Equal(item.Status, DeductionRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionRollupMovesToSuspended()
    {
        var item = DeductionRollup.NewDeductionRollup("code-190", "name-688");
        try
        {
            item.MoveTo(DeductionRollupStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionRollupStatus.Suspended, "status");
    }

    public static void DeductionRollupRejectsMovesFromClosed()
    {
        var item = DeductionRollup.NewDeductionRollup("code-190", "name-688");
        item.Status = DeductionRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(DeductionRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeductionRollupStartsInDraft();
        DeductionRollupMovesToSuspended();
        DeductionRollupRejectsMovesFromClosed();
    }
}
