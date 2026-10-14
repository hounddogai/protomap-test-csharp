namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedFeedbackRuleChecks
{
    public static void CappedFeedbackRuleStartsInDraft()
    {
        var item = CappedFeedbackRule.NewCappedFeedbackRule("code-934", "label-501");
        Check.Equal(item.Status, CappedFeedbackRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedFeedbackRuleMovesToActive()
    {
        var item = CappedFeedbackRule.NewCappedFeedbackRule("code-934", "label-501");
        try
        {
            item.MoveTo(CappedFeedbackRuleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedFeedbackRuleStatus.Active, "status");
    }

    public static void CappedFeedbackRuleRejectsMovesFromClosed()
    {
        var item = CappedFeedbackRule.NewCappedFeedbackRule("code-934", "label-501");
        item.Status = CappedFeedbackRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedFeedbackRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedFeedbackRuleStartsInDraft();
        CappedFeedbackRuleMovesToActive();
        CappedFeedbackRuleRejectsMovesFromClosed();
    }
}
