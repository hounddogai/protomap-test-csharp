namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HeadcountRuleChecks
{
    public static void HeadcountRuleStartsInPlanned()
    {
        var item = HeadcountRule.NewHeadcountRule("code-661", "name-957");
        Check.Equal(item.Status, HeadcountRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountRuleMovesToCompleted()
    {
        var item = HeadcountRule.NewHeadcountRule("code-661", "name-957");
        try
        {
            item.MoveTo(HeadcountRuleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountRuleStatus.Completed, "status");
    }

    public static void HeadcountRuleRejectsMovesFromCancelled()
    {
        var item = HeadcountRule.NewHeadcountRule("code-661", "name-957");
        item.Status = HeadcountRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HeadcountRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HeadcountRuleStartsInPlanned();
        HeadcountRuleMovesToCompleted();
        HeadcountRuleRejectsMovesFromCancelled();
    }
}
