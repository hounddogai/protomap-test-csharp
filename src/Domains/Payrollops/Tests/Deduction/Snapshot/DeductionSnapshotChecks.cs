namespace Acme.Workforce.Payrollops.Tests.Deduction.Snapshot;

using Acme.Workforce.Payrollops.Deduction.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DeductionSnapshotChecks
{
    public static void DeductionSnapshotStartsInIdle()
    {
        var item = DeductionSnapshot.NewDeductionSnapshot("code-912", "label-91");
        Check.Equal(item.Status, DeductionSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionSnapshotMovesToReady()
    {
        var item = DeductionSnapshot.NewDeductionSnapshot("code-912", "label-91");
        try
        {
            item.MoveTo(DeductionSnapshotStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionSnapshotStatus.Ready, "status");
    }

    public static void DeductionSnapshotRejectsMovesFromStopped()
    {
        var item = DeductionSnapshot.NewDeductionSnapshot("code-912", "label-91");
        item.Status = DeductionSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionSnapshotStartsInIdle();
        DeductionSnapshotMovesToReady();
        DeductionSnapshotRejectsMovesFromStopped();
    }
}
