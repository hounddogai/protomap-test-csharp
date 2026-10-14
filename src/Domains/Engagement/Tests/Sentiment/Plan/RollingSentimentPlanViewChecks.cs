namespace Acme.Workforce.Engagement.Tests.Sentiment.Plan;

using Acme.Workforce.Engagement.Sentiment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingSentimentPlanViewChecks
{
    public static void RollingSentimentPlanViewShowsTheStatusInCapitals()
    {
        var item = RollingSentimentPlan.NewRollingSentimentPlan("code-890", "name-65");
        var view = RollingSentimentPlanView.ToRollingSentimentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingSentimentPlanView.ToRollingSentimentPlanViews(new List<RollingSentimentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingSentimentPlanViewShowsTheStatusInCapitals();
    }
}
