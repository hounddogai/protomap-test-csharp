namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GlobalFeedbackVarianceChecks
{
    public static void GlobalFeedbackVarianceStartsInOpen()
    {
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("code-851", "name-73");
        Check.Equal(item.Status, GlobalFeedbackVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalFeedbackVarianceMovesToArchived()
    {
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("code-851", "name-73");
        try
        {
            item.MoveTo(GlobalFeedbackVarianceStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalFeedbackVarianceStatus.Archived, "status");
    }

    public static void GlobalFeedbackVarianceRejectsMovesFromArchived()
    {
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("code-851", "name-73");
        item.Status = GlobalFeedbackVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalFeedbackVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalFeedbackVarianceStartsInOpen();
        GlobalFeedbackVarianceMovesToArchived();
        GlobalFeedbackVarianceRejectsMovesFromArchived();
    }
}
