namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FeedbackRuleChecks
{
    public static void FeedbackRuleStartsInNew()
    {
        var item = FeedbackRule.NewFeedbackRule("code-838", "name-142");
        Check.Equal(item.Status, FeedbackRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackRuleMovesToRunning()
    {
        var item = FeedbackRule.NewFeedbackRule("code-838", "name-142");
        try
        {
            item.MoveTo(FeedbackRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackRuleStatus.Running, "status");
    }

    public static void FeedbackRuleRejectsMovesFromFailed()
    {
        var item = FeedbackRule.NewFeedbackRule("code-838", "name-142");
        item.Status = FeedbackRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackRuleStartsInNew();
        FeedbackRuleMovesToRunning();
        FeedbackRuleRejectsMovesFromFailed();
    }
}
