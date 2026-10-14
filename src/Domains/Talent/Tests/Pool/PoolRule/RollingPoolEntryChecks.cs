namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class RollingPoolEntryChecks
{
    public static void RollingPoolEntryStartsInProposed()
    {
        var item = RollingPoolEntry.NewRollingPoolEntry("code-615", "title-533");
        Check.Equal(item.Status, RollingPoolEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPoolEntryMovesToRejected()
    {
        var item = RollingPoolEntry.NewRollingPoolEntry("code-615", "title-533");
        try
        {
            item.MoveTo(RollingPoolEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPoolEntryStatus.Rejected, "status");
    }

    public static void RollingPoolEntryRejectsMovesFromWithdrawn()
    {
        var item = RollingPoolEntry.NewRollingPoolEntry("code-615", "title-533");
        item.Status = RollingPoolEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingPoolEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingPoolEntryStartsInProposed();
        RollingPoolEntryMovesToRejected();
        RollingPoolEntryRejectsMovesFromWithdrawn();
    }
}
