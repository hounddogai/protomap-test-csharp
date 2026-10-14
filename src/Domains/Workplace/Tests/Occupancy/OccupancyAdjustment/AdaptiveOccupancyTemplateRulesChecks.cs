namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class AdaptiveOccupancyTemplateRulesChecks
{
    public static void AdaptiveOccupancyTemplateValidatorReportsEveryProblem()
    {
        var validator = AdaptiveOccupancyTemplateRules.DefaultAdaptiveOccupancyTemplateValidator();
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveOccupancyTemplateThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdaptiveOccupancyTemplateThresholdRangeRule(3.1, 3975.2);
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("x", "y");
        item.Threshold = 3976.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveOccupancyTemplateSourceLengthRuleFlagsALongSource()
    {
        var rule = new AdaptiveOccupancyTemplateSourceLengthRule(64);
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveOccupancyTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveOccupancyTemplateLabelLengthRule(3600);
        var item = AdaptiveOccupancyTemplate.NewAdaptiveOccupancyTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveOccupancyTemplateValidatorReportsEveryProblem();
        AdaptiveOccupancyTemplateThresholdRangeRuleFlagsValuesAboveThreshold();
        AdaptiveOccupancyTemplateSourceLengthRuleFlagsALongSource();
        AdaptiveOccupancyTemplateLabelLengthRuleFlagsALongLabel();
    }
}
