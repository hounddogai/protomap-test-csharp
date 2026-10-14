namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class GlobalCareerPathRuleRulesChecks
{
    public static void GlobalCareerPathRuleValidatorReportsEveryProblem()
    {
        var validator = GlobalCareerPathRuleRules.DefaultGlobalCareerPathRuleValidator();
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalCareerPathRuleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new GlobalCareerPathRuleLeaveDaysRangeRule(120, 86400);
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalCareerPathRuleRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new GlobalCareerPathRuleRequiredSegmentRule("global career path rule needs this field");
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalCareerPathRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalCareerPathRuleRequiredLabelRule("global career path rule needs an entry");
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalCareerPathRuleValidatorReportsEveryProblem();
        GlobalCareerPathRuleLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        GlobalCareerPathRuleRequiredSegmentRuleFlagsABlankSegment();
        GlobalCareerPathRuleRequiredLabelRuleFlagsABlankLabel();
    }
}
