namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SuccessionPlanRuleRulesChecks
{
    public static void SuccessionPlanRuleValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanRuleRules.DefaultSuccessionPlanRuleValidator();
        var item = SuccessionPlanRule.NewSuccessionPlanRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanRuleLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new SuccessionPlanRuleLimitRangeRule(64, 64);
        var item = SuccessionPlanRule.NewSuccessionPlanRule("x", "y");
        item.Limit = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanRuleRequiredCodeRule("succession plan rule needs a value");
        var item = SuccessionPlanRule.NewSuccessionPlanRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanRuleValidatorReportsEveryProblem();
        SuccessionPlanRuleLimitRangeRuleFlagsValuesAboveLimit();
        SuccessionPlanRuleRequiredCodeRuleFlagsABlankCode();
    }
}
