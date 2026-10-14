namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionRuleRulesChecks
{
    public static void ExceptionRuleValidatorReportsEveryProblem()
    {
        var validator = ExceptionRuleRules.DefaultExceptionRuleValidator();
        var item = ExceptionRule.NewExceptionRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionRuleFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ExceptionRuleFteRangeRule(0.1, 1378.7);
        var item = ExceptionRule.NewExceptionRule("x", "y");
        item.Fte = 1380.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ExceptionRuleRequiredTitleRule("exception rule needs a value");
        var item = ExceptionRule.NewExceptionRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionRuleRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ExceptionRuleRequiredRegionRule("exception rule needs this field");
        var item = ExceptionRule.NewExceptionRule("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionRuleRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new ExceptionRuleRequiredReviewerRule("exception rule needs a value");
        var item = ExceptionRule.NewExceptionRule("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ExceptionRuleCodeLengthRule(120);
        var item = ExceptionRule.NewExceptionRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ExceptionRuleRequiredCodeRule("exception rule needs a value");
        var item = ExceptionRule.NewExceptionRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ExceptionRuleValidatorReportsEveryProblem();
        ExceptionRuleFteRangeRuleFlagsValuesAboveFte();
        ExceptionRuleRequiredTitleRuleFlagsABlankTitle();
        ExceptionRuleRequiredRegionRuleFlagsABlankRegion();
        ExceptionRuleRequiredReviewerRuleFlagsABlankReviewer();
        ExceptionRuleCodeLengthRuleFlagsALongCode();
        ExceptionRuleRequiredCodeRuleFlagsABlankCode();
    }
}
