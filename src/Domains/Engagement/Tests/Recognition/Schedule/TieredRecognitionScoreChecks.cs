namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredRecognitionScoreChecks
{
    public static void TieredRecognitionScoreStartsInDraft()
    {
        var item = TieredRecognitionScore.NewTieredRecognitionScore("code-61", "name-198");
        Check.Equal(item.Status, TieredRecognitionScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredRecognitionScoreMovesToActive()
    {
        var item = TieredRecognitionScore.NewTieredRecognitionScore("code-61", "name-198");
        try
        {
            item.MoveTo(TieredRecognitionScoreStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredRecognitionScoreStatus.Active, "status");
    }

    public static void TieredRecognitionScoreRejectsMovesFromClosed()
    {
        var item = TieredRecognitionScore.NewTieredRecognitionScore("code-61", "name-198");
        item.Status = TieredRecognitionScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredRecognitionScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredRecognitionScoreStartsInDraft();
        TieredRecognitionScoreMovesToActive();
        TieredRecognitionScoreRejectsMovesFromClosed();
    }
}
