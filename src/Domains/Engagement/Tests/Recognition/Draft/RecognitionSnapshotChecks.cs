namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RecognitionSnapshotChecks
{
    public static void RecognitionSnapshotStartsInNew()
    {
        var item = RecognitionSnapshot.NewRecognitionSnapshot("code-380", "label-895");
        Check.Equal(item.Status, RecognitionSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionSnapshotMovesToDone()
    {
        var item = RecognitionSnapshot.NewRecognitionSnapshot("code-380", "label-895");
        try
        {
            item.MoveTo(RecognitionSnapshotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionSnapshotStatus.Done, "status");
    }

    public static void RecognitionSnapshotRejectsMovesFromFailed()
    {
        var item = RecognitionSnapshot.NewRecognitionSnapshot("code-380", "label-895");
        item.Status = RecognitionSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(RecognitionSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        RecognitionSnapshotStartsInNew();
        RecognitionSnapshotMovesToDone();
        RecognitionSnapshotRejectsMovesFromFailed();
    }
}
