namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceRuleChecks
{
    public static void BalanceRuleStartsInCreated()
    {
        var item = BalanceRule.NewBalanceRule("code-156", "title-590");
        Check.Equal(item.Status, BalanceRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceRuleMovesToRetired()
    {
        var item = BalanceRule.NewBalanceRule("code-156", "title-590");
        try
        {
            item.MoveTo(BalanceRuleStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceRuleStatus.Retired, "status");
    }

    public static void BalanceRuleRejectsMovesFromRetired()
    {
        var item = BalanceRule.NewBalanceRule("code-156", "title-590");
        item.Status = BalanceRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(BalanceRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        BalanceRuleStartsInCreated();
        BalanceRuleMovesToRetired();
        BalanceRuleRejectsMovesFromRetired();
    }
}
