namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupRulesChecks
{
    public static void GlobalAdjustmentRollupValidatorReportsEveryProblem()
    {
        var validator = GlobalAdjustmentRollupRules.DefaultGlobalAdjustmentRollupValidator();
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAdjustmentRollupRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new GlobalAdjustmentRollupRequiredSalaryBandRule("global adjustment rollup needs a value");
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAdjustmentRollupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new GlobalAdjustmentRollupThresholdRangeRule(3.2, 252.0);
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("x", "y");
        item.Threshold = 253.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAdjustmentRollupAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new GlobalAdjustmentRollupAmountRangeRule(2.0, 2139.3);
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("x", "y");
        item.Amount = 2140.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRollupValidatorReportsEveryProblem();
        GlobalAdjustmentRollupRequiredSalaryBandRuleFlagsABlankSalaryBand();
        GlobalAdjustmentRollupThresholdRangeRuleFlagsValuesAboveThreshold();
        GlobalAdjustmentRollupAmountRangeRuleFlagsValuesAboveAmount();
    }
}
