namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualEntryChecks
{
    public static void AccrualEntryStartsInProposed()
    {
        var item = AccrualEntry.NewAccrualEntry("code-68", "label-464");
        Check.Equal(item.Status, AccrualEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualEntryMovesToApproved()
    {
        var item = AccrualEntry.NewAccrualEntry("code-68", "label-464");
        try
        {
            item.MoveTo(AccrualEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualEntryStatus.Approved, "status");
    }

    public static void AccrualEntryRejectsMovesFromWithdrawn()
    {
        var item = AccrualEntry.NewAccrualEntry("code-68", "label-464");
        item.Status = AccrualEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AccrualEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AccrualEntryStartsInProposed();
        AccrualEntryMovesToApproved();
        AccrualEntryRejectsMovesFromWithdrawn();
    }
}
