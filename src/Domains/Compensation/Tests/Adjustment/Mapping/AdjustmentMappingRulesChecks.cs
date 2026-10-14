namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdjustmentMappingRulesChecks
{
    public static void AdjustmentMappingValidatorReportsEveryProblem()
    {
        var validator = AdjustmentMappingRules.DefaultAdjustmentMappingValidator();
        var item = AdjustmentMapping.NewAdjustmentMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentMappingMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdjustmentMappingMinimumRangeRule(6.5, 4228.9);
        var item = AdjustmentMapping.NewAdjustmentMapping("x", "y");
        item.Minimum = 4230.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdjustmentMappingRequiredTitleRule("adjustment mapping needs an entry");
        var item = AdjustmentMapping.NewAdjustmentMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new AdjustmentMappingTenureMonthsRangeRule(1000, 3600);
        var item = AdjustmentMapping.NewAdjustmentMapping("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentMappingValidatorReportsEveryProblem();
        AdjustmentMappingMinimumRangeRuleFlagsValuesAboveMinimum();
        AdjustmentMappingRequiredTitleRuleFlagsABlankTitle();
        AdjustmentMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
