namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionEntryChecks
{
    public static void DeductionEntryStartsInProposed()
    {
        var item = DeductionEntry.NewDeductionEntry("code-439", "label-275");
        Check.Equal(item.Status, DeductionEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionEntryMovesToApproved()
    {
        var item = DeductionEntry.NewDeductionEntry("code-439", "label-275");
        try
        {
            item.MoveTo(DeductionEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionEntryStatus.Approved, "status");
    }

    public static void DeductionEntryRejectsMovesFromWithdrawn()
    {
        var item = DeductionEntry.NewDeductionEntry("code-439", "label-275");
        item.Status = DeductionEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionEntryStartsInProposed();
        DeductionEntryMovesToApproved();
        DeductionEntryRejectsMovesFromWithdrawn();
    }
}
