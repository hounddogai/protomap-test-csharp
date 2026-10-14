namespace Acme.Workforce.Engagement.Tests.Recognition.Window;

using Acme.Workforce.Engagement.Recognition.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RecognitionWindowChecks
{
    public static void RecognitionWindowStartsInPlanned()
    {
        var item = RecognitionWindow.NewRecognitionWindow("code-456", "label-176");
        Check.Equal(item.Status, RecognitionWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionWindowMovesToInProgress()
    {
        var item = RecognitionWindow.NewRecognitionWindow("code-456", "label-176");
        try
        {
            item.MoveTo(RecognitionWindowStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionWindowStatus.InProgress, "status");
    }

    public static void RecognitionWindowRejectsMovesFromCancelled()
    {
        var item = RecognitionWindow.NewRecognitionWindow("code-456", "label-176");
        item.Status = RecognitionWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RecognitionWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RecognitionWindowStartsInPlanned();
        RecognitionWindowMovesToInProgress();
        RecognitionWindowRejectsMovesFromCancelled();
    }
}
