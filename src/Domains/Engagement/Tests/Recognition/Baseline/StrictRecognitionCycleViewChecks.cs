namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictRecognitionCycleViewChecks
{
    public static void StrictRecognitionCycleViewShowsTheStatusInCapitals()
    {
        var item = StrictRecognitionCycle.NewStrictRecognitionCycle("code-942", "name-212");
        var view = StrictRecognitionCycleView.ToStrictRecognitionCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictRecognitionCycleView.ToStrictRecognitionCycleViews(new List<StrictRecognitionCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictRecognitionCycleViewShowsTheStatusInCapitals();
    }
}
