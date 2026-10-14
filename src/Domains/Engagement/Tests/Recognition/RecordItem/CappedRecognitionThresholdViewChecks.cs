namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class CappedRecognitionThresholdViewChecks
{
    public static void CappedRecognitionThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedRecognitionThreshold.NewCappedRecognitionThreshold("code-364", "title-193");
        var view = CappedRecognitionThresholdView.ToCappedRecognitionThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedRecognitionThresholdView.ToCappedRecognitionThresholdViews(new List<CappedRecognitionThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedRecognitionThresholdViewShowsTheStatusInCapitals();
    }
}
