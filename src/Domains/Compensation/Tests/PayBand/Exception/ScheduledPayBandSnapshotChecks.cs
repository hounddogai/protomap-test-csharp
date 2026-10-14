namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotChecks
{
    public static void ScheduledPayBandSnapshotStartsInProposed()
    {
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
        Check.Equal(item.Status, ScheduledPayBandSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayBandSnapshotMovesToWithdrawn()
    {
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
        try
        {
            item.MoveTo(ScheduledPayBandSnapshotStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayBandSnapshotStatus.Withdrawn, "status");
    }

    public static void ScheduledPayBandSnapshotRejectsMovesFromWithdrawn()
    {
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
        item.Status = ScheduledPayBandSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledPayBandSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledPayBandSnapshotStartsInProposed();
        ScheduledPayBandSnapshotMovesToWithdrawn();
        ScheduledPayBandSnapshotRejectsMovesFromWithdrawn();
    }
}
