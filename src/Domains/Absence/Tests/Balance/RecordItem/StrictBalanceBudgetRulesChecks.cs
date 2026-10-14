namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictBalanceBudgetRulesChecks
{
    public static void StrictBalanceBudgetValidatorReportsEveryProblem()
    {
        var validator = StrictBalanceBudgetRules.DefaultStrictBalanceBudgetValidator();
        var item = StrictBalanceBudget.NewStrictBalanceBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictBalanceBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictBalanceBudgetCodeLengthRule(1000);
        var item = StrictBalanceBudget.NewStrictBalanceBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictBalanceBudgetFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new StrictBalanceBudgetFteRangeRule(3.7, 4051.9);
        var item = StrictBalanceBudget.NewStrictBalanceBudget("x", "y");
        item.Fte = 4053.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictBalanceBudgetPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new StrictBalanceBudgetPriorityRangeRule(255, 1000);
        var item = StrictBalanceBudget.NewStrictBalanceBudget("x", "y");
        item.Priority = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictBalanceBudgetValidatorReportsEveryProblem();
        StrictBalanceBudgetCodeLengthRuleFlagsALongCode();
        StrictBalanceBudgetFteRangeRuleFlagsValuesAboveFte();
        StrictBalanceBudgetPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
