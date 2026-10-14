namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualRuleChecks
{
    public static void AccrualRuleStartsInOpen()
    {
        var item = AccrualRule.NewAccrualRule("code-311", "name-753");
        Check.Equal(item.Status, AccrualRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualRuleMovesToResolved()
    {
        var item = AccrualRule.NewAccrualRule("code-311", "name-753");
        try
        {
            item.MoveTo(AccrualRuleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualRuleStatus.Resolved, "status");
    }

    public static void AccrualRuleRejectsMovesFromArchived()
    {
        var item = AccrualRule.NewAccrualRule("code-311", "name-753");
        item.Status = AccrualRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(AccrualRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccrualRuleStartsInOpen();
        AccrualRuleMovesToResolved();
        AccrualRuleRejectsMovesFromArchived();
    }
}
