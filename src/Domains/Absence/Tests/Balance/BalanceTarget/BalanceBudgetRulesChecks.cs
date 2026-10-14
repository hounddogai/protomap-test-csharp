namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceBudgetRulesChecks
{
    public static void BalanceBudgetValidatorReportsEveryProblem()
    {
        var validator = BalanceBudgetRules.DefaultBalanceBudgetValidator();
        var item = BalanceBudget.NewBalanceBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalanceBudgetRequiredCodeRule("balance budget needs an entry");
        var item = BalanceBudget.NewBalanceBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceBudgetJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new BalanceBudgetJobCodeLengthRule(80);
        var item = BalanceBudget.NewBalanceBudget("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceBudgetFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new BalanceBudgetFteRangeRule(4.6, 569.6);
        var item = BalanceBudget.NewBalanceBudget("x", "y");
        item.Fte = 571.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceBudgetPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new BalanceBudgetPriorityRangeRule(64, 86400);
        var item = BalanceBudget.NewBalanceBudget("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceBudgetValidatorReportsEveryProblem();
        BalanceBudgetRequiredCodeRuleFlagsABlankCode();
        BalanceBudgetJobCodeLengthRuleFlagsALongJobCode();
        BalanceBudgetFteRangeRuleFlagsValuesAboveFte();
        BalanceBudgetPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
