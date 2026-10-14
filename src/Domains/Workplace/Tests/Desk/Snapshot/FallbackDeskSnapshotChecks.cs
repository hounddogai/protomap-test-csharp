namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackDeskSnapshotChecks
{
    public static void FallbackDeskSnapshotStartsInDraft()
    {
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
        Check.Equal(item.Status, FallbackDeskSnapshotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackDeskSnapshotMovesToPending()
    {
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
        try
        {
            item.MoveTo(FallbackDeskSnapshotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackDeskSnapshotStatus.Pending, "status");
    }

    public static void FallbackDeskSnapshotRejectsMovesFromClosed()
    {
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
        item.Status = FallbackDeskSnapshotStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackDeskSnapshotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackDeskSnapshotStartsInDraft();
        FallbackDeskSnapshotMovesToPending();
        FallbackDeskSnapshotRejectsMovesFromClosed();
    }
}
