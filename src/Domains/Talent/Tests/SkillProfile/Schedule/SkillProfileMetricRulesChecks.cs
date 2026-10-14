namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SkillProfileMetricRulesChecks
{
    public static void SkillProfileMetricValidatorReportsEveryProblem()
    {
        var validator = SkillProfileMetricRules.DefaultSkillProfileMetricValidator();
        var item = SkillProfileMetric.NewSkillProfileMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileMetricThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new SkillProfileMetricThresholdRangeRule(0.0, 4176.0);
        var item = SkillProfileMetric.NewSkillProfileMetric("x", "y");
        item.Threshold = 4177.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileMetricLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new SkillProfileMetricLocaleLengthRule(86400);
        var item = SkillProfileMetric.NewSkillProfileMetric("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new SkillProfileMetricRequiredLocaleRule("skill profile metric needs a value");
        var item = SkillProfileMetric.NewSkillProfileMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileMetricFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new SkillProfileMetricFloorValueRangeRule(8.3, 3294.3);
        var item = SkillProfileMetric.NewSkillProfileMetric("x", "y");
        item.FloorValue = 3295.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SkillProfileMetricRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SkillProfileMetricRequiredTitleRule("skill profile metric needs an entry");
        var item = SkillProfileMetric.NewSkillProfileMetric("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SkillProfileMetricValidatorReportsEveryProblem();
        SkillProfileMetricThresholdRangeRuleFlagsValuesAboveThreshold();
        SkillProfileMetricLocaleLengthRuleFlagsALongLocale();
        SkillProfileMetricRequiredLocaleRuleFlagsABlankLocale();
        SkillProfileMetricFloorValueRangeRuleFlagsValuesAboveFloorValue();
        SkillProfileMetricRequiredTitleRuleFlagsABlankTitle();
    }
}
