namespace Acme.Workforce.Engagement.Tests.Sentiment.Plan;

using Acme.Workforce.Engagement.Sentiment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingSentimentPlanChecks
{
    public static void RollingSentimentPlanStartsInDraft()
    {
        var item = RollingSentimentPlan.NewRollingSentimentPlan("code-890", "name-65");
        Check.Equal(item.Status, RollingSentimentPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSentimentPlanMovesToPending()
    {
        var item = RollingSentimentPlan.NewRollingSentimentPlan("code-890", "name-65");
        try
        {
            item.MoveTo(RollingSentimentPlanStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSentimentPlanStatus.Pending, "status");
    }

    public static void RollingSentimentPlanRejectsMovesFromClosed()
    {
        var item = RollingSentimentPlan.NewRollingSentimentPlan("code-890", "name-65");
        item.Status = RollingSentimentPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingSentimentPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingSentimentPlanStartsInDraft();
        RollingSentimentPlanMovesToPending();
        RollingSentimentPlanRejectsMovesFromClosed();
    }
}
