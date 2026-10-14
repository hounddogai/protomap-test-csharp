namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalRecognitionScoreChecks
{
    public static void GlobalRecognitionScoreStartsInDraft()
    {
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("code-701", "title-90");
        Check.Equal(item.Status, GlobalRecognitionScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalRecognitionScoreMovesToPending()
    {
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("code-701", "title-90");
        try
        {
            item.MoveTo(GlobalRecognitionScoreStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalRecognitionScoreStatus.Pending, "status");
    }

    public static void GlobalRecognitionScoreRejectsMovesFromClosed()
    {
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("code-701", "title-90");
        item.Status = GlobalRecognitionScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalRecognitionScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalRecognitionScoreStartsInDraft();
        GlobalRecognitionScoreMovesToPending();
        GlobalRecognitionScoreRejectsMovesFromClosed();
    }
}
