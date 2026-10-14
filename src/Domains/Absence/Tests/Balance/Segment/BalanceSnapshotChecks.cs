namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceSnapshotChecks
{
    public static void BalanceSnapshotStartsInProposed()
    {
        var item = BalanceSnapshot.NewBalanceSnapshot("code-151", "label-965");
        Check.Equal(item.Status, BalanceSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceSnapshotMovesToWithdrawn()
    {
        var item = BalanceSnapshot.NewBalanceSnapshot("code-151", "label-965");
        try
        {
            item.MoveTo(BalanceSnapshotStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceSnapshotStatus.Withdrawn, "status");
    }

    public static void BalanceSnapshotRejectsMovesFromWithdrawn()
    {
        var item = BalanceSnapshot.NewBalanceSnapshot("code-151", "label-965");
        item.Status = BalanceSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BalanceSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BalanceSnapshotStartsInProposed();
        BalanceSnapshotMovesToWithdrawn();
        BalanceSnapshotRejectsMovesFromWithdrawn();
    }
}
