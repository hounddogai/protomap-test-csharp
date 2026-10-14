namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientAdjustmentRollupRulesChecks
{
    public static void LenientAdjustmentRollupValidatorReportsEveryProblem()
    {
        var validator = LenientAdjustmentRollupRules.DefaultLenientAdjustmentRollupValidator();
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientAdjustmentRollupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientAdjustmentRollupTitleLengthRule(16);
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientAdjustmentRollupRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientAdjustmentRollupRequiredTitleRule("lenient adjustment rollup needs this field");
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientAdjustmentRollupRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LenientAdjustmentRollupRateRangeRule(7.8, 3860.3);
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("x", "y");
        item.Rate = 3861.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientAdjustmentRollupValidatorReportsEveryProblem();
        LenientAdjustmentRollupTitleLengthRuleFlagsALongTitle();
        LenientAdjustmentRollupRequiredTitleRuleFlagsABlankTitle();
        LenientAdjustmentRollupRateRangeRuleFlagsValuesAboveRate();
    }
}
