namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DefaultVisitorRuleChecks
{
    public static void DefaultVisitorRuleStartsInOpen()
    {
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
        Check.Equal(item.Status, DefaultVisitorRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVisitorRuleMovesToInReview()
    {
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
        try
        {
            item.MoveTo(DefaultVisitorRuleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVisitorRuleStatus.InReview, "status");
    }

    public static void DefaultVisitorRuleRejectsMovesFromArchived()
    {
        var item = DefaultVisitorRule.NewDefaultVisitorRule("code-619", "label-151");
        item.Status = DefaultVisitorRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultVisitorRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultVisitorRuleStartsInOpen();
        DefaultVisitorRuleMovesToInReview();
        DefaultVisitorRuleRejectsMovesFromArchived();
    }
}
