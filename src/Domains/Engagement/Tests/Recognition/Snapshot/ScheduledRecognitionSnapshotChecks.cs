namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledRecognitionSnapshotChecks
{
    public static void ScheduledRecognitionSnapshotStartsInPlanned()
    {
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("code-862", "title-413");
        Check.Equal(item.Status, ScheduledRecognitionSnapshotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRecognitionSnapshotMovesToCancelled()
    {
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("code-862", "title-413");
        try
        {
            item.MoveTo(ScheduledRecognitionSnapshotStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRecognitionSnapshotStatus.Cancelled, "status");
    }

    public static void ScheduledRecognitionSnapshotRejectsMovesFromCancelled()
    {
        var item = ScheduledRecognitionSnapshot.NewScheduledRecognitionSnapshot("code-862", "title-413");
        item.Status = ScheduledRecognitionSnapshotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledRecognitionSnapshotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledRecognitionSnapshotStartsInPlanned();
        ScheduledRecognitionSnapshotMovesToCancelled();
        ScheduledRecognitionSnapshotRejectsMovesFromCancelled();
    }
}
