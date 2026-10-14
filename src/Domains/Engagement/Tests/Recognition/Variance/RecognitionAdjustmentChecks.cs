namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionAdjustmentChecks
{
    public static void RecognitionAdjustmentStartsInDraft()
    {
        var item = RecognitionAdjustment.NewRecognitionAdjustment("code-333", "name-875");
        Check.Equal(item.Status, RecognitionAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionAdjustmentMovesToPending()
    {
        var item = RecognitionAdjustment.NewRecognitionAdjustment("code-333", "name-875");
        try
        {
            item.MoveTo(RecognitionAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionAdjustmentStatus.Pending, "status");
    }

    public static void RecognitionAdjustmentRejectsMovesFromClosed()
    {
        var item = RecognitionAdjustment.NewRecognitionAdjustment("code-333", "name-875");
        item.Status = RecognitionAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionAdjustmentStartsInDraft();
        RecognitionAdjustmentMovesToPending();
        RecognitionAdjustmentRejectsMovesFromClosed();
    }
}
