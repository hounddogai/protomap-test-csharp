namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorSnapshotChecks
{
    public static void VisitorSnapshotStartsInIdle()
    {
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
        Check.Equal(item.Status, VisitorSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorSnapshotMovesToDraining()
    {
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
        try
        {
            item.MoveTo(VisitorSnapshotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorSnapshotStatus.Draining, "status");
    }

    public static void VisitorSnapshotRejectsMovesFromStopped()
    {
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
        item.Status = VisitorSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorSnapshotStartsInIdle();
        VisitorSnapshotMovesToDraining();
        VisitorSnapshotRejectsMovesFromStopped();
    }
}
