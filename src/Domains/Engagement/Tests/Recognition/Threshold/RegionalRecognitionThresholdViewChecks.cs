namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalRecognitionThresholdViewChecks
{
    public static void RegionalRecognitionThresholdViewShowsTheStatusInCapitals()
    {
        var item = RegionalRecognitionThreshold.NewRegionalRecognitionThreshold("code-150", "label-902");
        var view = RegionalRecognitionThresholdView.ToRegionalRecognitionThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalRecognitionThresholdView.ToRegionalRecognitionThresholdViews(new List<RegionalRecognitionThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalRecognitionThresholdViewShowsTheStatusInCapitals();
    }
}
