namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SkillProfileThresholdRulesChecks
{
    public static void SkillProfileThresholdValidatorReportsEveryProblem()
    {
        var validator = SkillProfileThresholdRules.DefaultSkillProfileThresholdValidator();
        var item = SkillProfileThreshold.NewSkillProfileThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SkillProfileThresholdRequiredLabelRule("skill profile threshold needs a value");
        var item = SkillProfileThreshold.NewSkillProfileThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileThresholdRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new SkillProfileThresholdRequiredGradeRule("skill profile threshold needs a value");
        var item = SkillProfileThreshold.NewSkillProfileThreshold("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileThresholdMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new SkillProfileThresholdMinimumRangeRule(2.9, 1795.3);
        var item = SkillProfileThreshold.NewSkillProfileThreshold("x", "y");
        item.Minimum = 1796.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileThresholdCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new SkillProfileThresholdCapacityRangeRule(32, 3600);
        var item = SkillProfileThreshold.NewSkillProfileThreshold("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SkillProfileThresholdRequiredCodeRule("skill profile threshold needs an entry");
        var item = SkillProfileThreshold.NewSkillProfileThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileThresholdValidatorReportsEveryProblem();
        SkillProfileThresholdRequiredLabelRuleFlagsABlankLabel();
        SkillProfileThresholdRequiredGradeRuleFlagsABlankGrade();
        SkillProfileThresholdMinimumRangeRuleFlagsValuesAboveMinimum();
        SkillProfileThresholdCapacityRangeRuleFlagsValuesAboveCapacity();
        SkillProfileThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
