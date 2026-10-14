namespace Acme.Workforce.Payrollops.Tests.Deduction.Slot;

using Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeductionBundleChecks
{
    public static void DeductionBundleStartsInNew()
    {
        var item = DeductionBundle.NewDeductionBundle("code-799", "name-899");
        Check.Equal(item.Status, DeductionBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionBundleMovesToRunning()
    {
        var item = DeductionBundle.NewDeductionBundle("code-799", "name-899");
        try
        {
            item.MoveTo(DeductionBundleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionBundleStatus.Running, "status");
    }

    public static void DeductionBundleRejectsMovesFromFailed()
    {
        var item = DeductionBundle.NewDeductionBundle("code-799", "name-899");
        item.Status = DeductionBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(DeductionBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        DeductionBundleStartsInNew();
        DeductionBundleMovesToRunning();
        DeductionBundleRejectsMovesFromFailed();
    }
}
