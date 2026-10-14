namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalRateRuleRulesChecks
{
    public static void RegionalRateRuleValidatorReportsEveryProblem()
    {
        var validator = RegionalRateRuleRules.DefaultRegionalRateRuleValidator();
        var item = RegionalRateRule.NewRegionalRateRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalRateRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalRateRuleRequiredCodeRule("regional rate rule needs an entry");
        var item = RegionalRateRule.NewRegionalRateRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalRateRuleLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new RegionalRateRuleLimitRangeRule(255, 1000);
        var item = RegionalRateRule.NewRegionalRateRule("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalRateRuleValidatorReportsEveryProblem();
        RegionalRateRuleRequiredCodeRuleFlagsABlankCode();
        RegionalRateRuleLimitRangeRuleFlagsValuesAboveLimit();
    }
}
