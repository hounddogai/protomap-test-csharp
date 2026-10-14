namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LenientRecognitionEntryChecks
{
    public static void LenientRecognitionEntryStartsInPlanned()
    {
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("code-814", "title-357");
        Check.Equal(item.Status, LenientRecognitionEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRecognitionEntryMovesToInProgress()
    {
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("code-814", "title-357");
        try
        {
            item.MoveTo(LenientRecognitionEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRecognitionEntryStatus.InProgress, "status");
    }

    public static void LenientRecognitionEntryRejectsMovesFromCancelled()
    {
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("code-814", "title-357");
        item.Status = LenientRecognitionEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientRecognitionEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientRecognitionEntryStartsInPlanned();
        LenientRecognitionEntryMovesToInProgress();
        LenientRecognitionEntryRejectsMovesFromCancelled();
    }
}
