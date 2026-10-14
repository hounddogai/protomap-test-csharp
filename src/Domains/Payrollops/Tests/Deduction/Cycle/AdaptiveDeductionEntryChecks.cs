namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveDeductionEntryChecks
{
    public static void AdaptiveDeductionEntryStartsInIdle()
    {
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("code-83", "title-871");
        Check.Equal(item.Status, AdaptiveDeductionEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveDeductionEntryMovesToStopped()
    {
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("code-83", "title-871");
        try
        {
            item.MoveTo(AdaptiveDeductionEntryStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveDeductionEntryStatus.Stopped, "status");
    }

    public static void AdaptiveDeductionEntryRejectsMovesFromStopped()
    {
        var item = AdaptiveDeductionEntry.NewAdaptiveDeductionEntry("code-83", "title-871");
        item.Status = AdaptiveDeductionEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveDeductionEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveDeductionEntryStartsInIdle();
        AdaptiveDeductionEntryMovesToStopped();
        AdaptiveDeductionEntryRejectsMovesFromStopped();
    }
}
