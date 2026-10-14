namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackRecognitionBaselineViewChecks
{
    public static void FallbackRecognitionBaselineViewShowsTheStatusInCapitals()
    {
        var item = FallbackRecognitionBaseline.NewFallbackRecognitionBaseline("code-827", "name-523");
        var view = FallbackRecognitionBaselineView.ToFallbackRecognitionBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackRecognitionBaselineView.ToFallbackRecognitionBaselineViews(new List<FallbackRecognitionBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackRecognitionBaselineViewShowsTheStatusInCapitals();
    }
}
