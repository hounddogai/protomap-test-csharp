namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualAccrualEntryChecks
{
    public static void ManualAccrualEntryStartsInOpen()
    {
        var item = ManualAccrualEntry.NewManualAccrualEntry("code-663", "label-971");
        Check.Equal(item.Status, ManualAccrualEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAccrualEntryMovesToResolved()
    {
        var item = ManualAccrualEntry.NewManualAccrualEntry("code-663", "label-971");
        try
        {
            item.MoveTo(ManualAccrualEntryStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAccrualEntryStatus.Resolved, "status");
    }

    public static void ManualAccrualEntryRejectsMovesFromArchived()
    {
        var item = ManualAccrualEntry.NewManualAccrualEntry("code-663", "label-971");
        item.Status = ManualAccrualEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualAccrualEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualAccrualEntryStartsInOpen();
        ManualAccrualEntryMovesToResolved();
        ManualAccrualEntryRejectsMovesFromArchived();
    }
}
