namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionPlanViewChecks
{
    public static void RecognitionPlanViewShowsTheStatusInCapitals()
    {
        var item = RecognitionPlan.NewRecognitionPlan("code-574", "title-883");
        var view = RecognitionPlanView.ToRecognitionPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionPlanView.ToRecognitionPlanViews(new List<RecognitionPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionPlanViewShowsTheStatusInCapitals();
    }
}
