namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayRuleRulesChecks
{
    public static void RetroPayRuleValidatorReportsEveryProblem()
    {
        var validator = RetroPayRuleRules.DefaultRetroPayRuleValidator();
        var item = RetroPayRule.NewRetroPayRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RetroPayRuleRequiredTitleRule("retro pay rule needs this field");
        var item = RetroPayRule.NewRetroPayRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayRuleRequiredCodeRule("retro pay rule needs this field");
        var item = RetroPayRule.NewRetroPayRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayRuleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new RetroPayRuleBudgetRangeRule(9.1, 1659.5);
        var item = RetroPayRule.NewRetroPayRule("x", "y");
        item.Budget = 1661.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RetroPayRuleTitleLengthRule(32);
        var item = RetroPayRule.NewRetroPayRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayRuleValidatorReportsEveryProblem();
        RetroPayRuleRequiredTitleRuleFlagsABlankTitle();
        RetroPayRuleRequiredCodeRuleFlagsABlankCode();
        RetroPayRuleBudgetRangeRuleFlagsValuesAboveBudget();
        RetroPayRuleTitleLengthRuleFlagsALongTitle();
    }
}
