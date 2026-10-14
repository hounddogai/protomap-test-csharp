namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RecognitionMetricViewChecks
{
    public static void RecognitionMetricViewShowsTheStatusInCapitals()
    {
        var item = RecognitionMetric.NewRecognitionMetric("code-746", "name-600");
        var view = RecognitionMetricView.ToRecognitionMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionMetricView.ToRecognitionMetricViews(new List<RecognitionMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionMetricViewShowsTheStatusInCapitals();
    }
}
