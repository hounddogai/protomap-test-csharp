namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class DefaultFeedbackRuleChecks
{
    public static void DefaultFeedbackRuleStartsInNew()
    {
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("code-992", "label-691");
        Check.Equal(item.Status, DefaultFeedbackRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultFeedbackRuleMovesToRunning()
    {
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("code-992", "label-691");
        try
        {
            item.MoveTo(DefaultFeedbackRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultFeedbackRuleStatus.Running, "status");
    }

    public static void DefaultFeedbackRuleRejectsMovesFromFailed()
    {
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("code-992", "label-691");
        item.Status = DefaultFeedbackRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultFeedbackRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultFeedbackRuleStartsInNew();
        DefaultFeedbackRuleMovesToRunning();
        DefaultFeedbackRuleRejectsMovesFromFailed();
    }
}
