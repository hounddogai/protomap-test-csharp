namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionCycleViewChecks
{
    public static void RecognitionCycleViewShowsTheStatusInCapitals()
    {
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
        var view = RecognitionCycleView.ToRecognitionCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionCycleView.ToRecognitionCycleViews(new List<RecognitionCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionCycleViewShowsTheStatusInCapitals();
    }
}
