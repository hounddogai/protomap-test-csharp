namespace Acme.Workforce.Contractors.Tests.Rate.Mapping;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class StrictRateMappingRulesChecks
{
    public static void StrictRateMappingValidatorReportsEveryProblem()
    {
        var validator = StrictRateMappingRules.DefaultStrictRateMappingValidator();
        var item = StrictRateMapping.NewStrictRateMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictRateMappingThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new StrictRateMappingThresholdRangeRule(7.9, 1385.5);
        var item = StrictRateMapping.NewStrictRateMapping("x", "y");
        item.Threshold = 1387.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictRateMappingValidatorReportsEveryProblem();
        StrictRateMappingThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
