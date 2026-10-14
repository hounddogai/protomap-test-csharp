namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionOverrideChecks
{
    public static void RecognitionOverrideStartsInPlanned()
    {
        var item = RecognitionOverride.NewRecognitionOverride("code-725", "label-571");
        Check.Equal(item.Status, RecognitionOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionOverrideMovesToInProgress()
    {
        var item = RecognitionOverride.NewRecognitionOverride("code-725", "label-571");
        try
        {
            item.MoveTo(RecognitionOverrideStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionOverrideStatus.InProgress, "status");
    }

    public static void RecognitionOverrideRejectsMovesFromCancelled()
    {
        var item = RecognitionOverride.NewRecognitionOverride("code-725", "label-571");
        item.Status = RecognitionOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RecognitionOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RecognitionOverrideStartsInPlanned();
        RecognitionOverrideMovesToInProgress();
        RecognitionOverrideRejectsMovesFromCancelled();
    }
}
