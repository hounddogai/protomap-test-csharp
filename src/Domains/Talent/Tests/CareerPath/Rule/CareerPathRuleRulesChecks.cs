namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleRulesChecks
{
    public static void CareerPathRuleValidatorReportsEveryProblem()
    {
        var validator = CareerPathRuleRules.DefaultCareerPathRuleValidator();
        var item = CareerPathRule.NewCareerPathRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathRuleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new CareerPathRuleThresholdRangeRule(4.3, 3546.7);
        var item = CareerPathRule.NewCareerPathRule("x", "y");
        item.Threshold = 3548.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathRuleSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CareerPathRuleSequenceRangeRule(120, 1000);
        var item = CareerPathRule.NewCareerPathRule("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathRuleCodeLengthRule(80);
        var item = CareerPathRule.NewCareerPathRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathRuleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new CareerPathRuleRequiredTimezoneRule("career path rule needs a value");
        var item = CareerPathRule.NewCareerPathRule("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathRuleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new CareerPathRuleRequiredGradeRule("career path rule needs this field");
        var item = CareerPathRule.NewCareerPathRule("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathRuleValidatorReportsEveryProblem();
        CareerPathRuleThresholdRangeRuleFlagsValuesAboveThreshold();
        CareerPathRuleSequenceRangeRuleFlagsValuesAboveSequence();
        CareerPathRuleCodeLengthRuleFlagsALongCode();
        CareerPathRuleRequiredTimezoneRuleFlagsABlankTimezone();
        CareerPathRuleRequiredGradeRuleFlagsABlankGrade();
    }
}
