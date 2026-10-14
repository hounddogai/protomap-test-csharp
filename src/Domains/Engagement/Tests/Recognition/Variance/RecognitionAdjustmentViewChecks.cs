namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionAdjustmentViewChecks
{
    public static void RecognitionAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RecognitionAdjustment.NewRecognitionAdjustment("code-333", "name-875");
        var view = RecognitionAdjustmentView.ToRecognitionAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionAdjustmentView.ToRecognitionAdjustmentViews(new List<RecognitionAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionAdjustmentViewShowsTheStatusInCapitals();
    }
}
