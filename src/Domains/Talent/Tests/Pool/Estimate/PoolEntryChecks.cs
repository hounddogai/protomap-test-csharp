namespace Acme.Workforce.Talent.Tests.Pool.Estimate;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PoolEntryChecks
{
    public static void PoolEntryStartsInProposed()
    {
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
        Check.Equal(item.Status, PoolEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolEntryMovesToRejected()
    {
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
        try
        {
            item.MoveTo(PoolEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolEntryStatus.Rejected, "status");
    }

    public static void PoolEntryRejectsMovesFromWithdrawn()
    {
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
        item.Status = PoolEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PoolEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PoolEntryStartsInProposed();
        PoolEntryMovesToRejected();
        PoolEntryRejectsMovesFromWithdrawn();
    }
}
