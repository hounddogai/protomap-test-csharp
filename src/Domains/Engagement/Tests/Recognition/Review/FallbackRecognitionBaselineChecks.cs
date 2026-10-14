namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackRecognitionBaselineChecks
{
    public static void FallbackRecognitionBaselineStartsInDraft()
    {
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("code-827", "name-523");
        Check.Equal(item.Status, FallbackRecognitionBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRecognitionBaselineMovesToPending()
    {
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("code-827", "name-523");
        try
        {
            item.MoveTo(FallbackRecognitionBaselineStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRecognitionBaselineStatus.Pending, "status");
    }

    public static void FallbackRecognitionBaselineRejectsMovesFromClosed()
    {
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("code-827", "name-523");
        item.Status = FallbackRecognitionBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackRecognitionBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackRecognitionBaselineStartsInDraft();
        FallbackRecognitionBaselineMovesToPending();
        FallbackRecognitionBaselineRejectsMovesFromClosed();
    }
}
