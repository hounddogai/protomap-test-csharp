namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RollingDeductionEntryChecks
{
    public static void RollingDeductionEntryStartsInIdle()
    {
        var item = RollingDeductionEntry.NewRollingDeductionEntry("code-822", "title-31");
        Check.Equal(item.Status, RollingDeductionEntryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingDeductionEntryMovesToWarming()
    {
        var item = RollingDeductionEntry.NewRollingDeductionEntry("code-822", "title-31");
        try
        {
            item.MoveTo(RollingDeductionEntryStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingDeductionEntryStatus.Warming, "status");
    }

    public static void RollingDeductionEntryRejectsMovesFromStopped()
    {
        var item = RollingDeductionEntry.NewRollingDeductionEntry("code-822", "title-31");
        item.Status = RollingDeductionEntryStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingDeductionEntryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingDeductionEntryStartsInIdle();
        RollingDeductionEntryMovesToWarming();
        RollingDeductionEntryRejectsMovesFromStopped();
    }
}
