namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackPayBandSnapshotChecks
{
    public static void FallbackPayBandSnapshotStartsInProposed()
    {
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("code-520", "title-3");
        Check.Equal(item.Status, FallbackPayBandSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPayBandSnapshotMovesToWithdrawn()
    {
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("code-520", "title-3");
        try
        {
            item.MoveTo(FallbackPayBandSnapshotStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPayBandSnapshotStatus.Withdrawn, "status");
    }

    public static void FallbackPayBandSnapshotRejectsMovesFromWithdrawn()
    {
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("code-520", "title-3");
        item.Status = FallbackPayBandSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackPayBandSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackPayBandSnapshotStartsInProposed();
        FallbackPayBandSnapshotMovesToWithdrawn();
        FallbackPayBandSnapshotRejectsMovesFromWithdrawn();
    }
}
