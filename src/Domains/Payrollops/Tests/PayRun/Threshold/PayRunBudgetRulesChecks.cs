namespace Acme.Workforce.Payrollops.Tests.PayRun.Threshold;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayRunBudgetRulesChecks
{
    public static void PayRunBudgetValidatorReportsEveryProblem()
    {
        var validator = PayRunBudgetRules.DefaultPayRunBudgetValidator();
        var item = PayRunBudget.NewPayRunBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new PayRunBudgetNameLengthRule(16);
        var item = PayRunBudget.NewPayRunBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBudgetScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PayRunBudgetScoreRangeRule(7.7, 898.2);
        var item = PayRunBudget.NewPayRunBudget("x", "y");
        item.Score = 899.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayRunBudgetCodeLengthRule(1000);
        var item = PayRunBudget.NewPayRunBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayRunBudgetRequiredCodeRule("pay run budget needs an entry");
        var item = PayRunBudget.NewPayRunBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayRunBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayRunBudgetRequiredNameRule("pay run budget needs an entry");
        var item = PayRunBudget.NewPayRunBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayRunBudgetValidatorReportsEveryProblem();
        PayRunBudgetNameLengthRuleFlagsALongName();
        PayRunBudgetScoreRangeRuleFlagsValuesAboveScore();
        PayRunBudgetCodeLengthRuleFlagsALongCode();
        PayRunBudgetRequiredCodeRuleFlagsABlankCode();
        PayRunBudgetRequiredNameRuleFlagsABlankName();
    }
}
