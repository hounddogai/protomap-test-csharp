namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolEntryChecks
{
    public static void BonusPoolEntryStartsInProposed()
    {
        var item = BonusPoolEntry.NewBonusPoolEntry("code-279", "name-136");
        Check.Equal(item.Status, BonusPoolEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolEntryMovesToApproved()
    {
        var item = BonusPoolEntry.NewBonusPoolEntry("code-279", "name-136");
        try
        {
            item.MoveTo(BonusPoolEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolEntryStatus.Approved, "status");
    }

    public static void BonusPoolEntryRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolEntry.NewBonusPoolEntry("code-279", "name-136");
        item.Status = BonusPoolEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolEntryStartsInProposed();
        BonusPoolEntryMovesToApproved();
        BonusPoolEntryRejectsMovesFromWithdrawn();
    }
}
