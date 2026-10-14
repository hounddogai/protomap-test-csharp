namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionOverrideChecks
{
    public static void DeductionOverrideStartsInPlanned()
    {
        var item = DeductionOverride.NewDeductionOverride("code-570", "name-984");
        Check.Equal(item.Status, DeductionOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionOverrideMovesToScheduled()
    {
        var item = DeductionOverride.NewDeductionOverride("code-570", "name-984");
        try
        {
            item.MoveTo(DeductionOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionOverrideStatus.Scheduled, "status");
    }

    public static void DeductionOverrideRejectsMovesFromCancelled()
    {
        var item = DeductionOverride.NewDeductionOverride("code-570", "name-984");
        item.Status = DeductionOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DeductionOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DeductionOverrideStartsInPlanned();
        DeductionOverrideMovesToScheduled();
        DeductionOverrideRejectsMovesFromCancelled();
    }
}
