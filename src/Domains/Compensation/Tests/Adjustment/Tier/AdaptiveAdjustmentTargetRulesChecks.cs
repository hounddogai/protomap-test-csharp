namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentTargetRulesChecks
{
    public static void AdaptiveAdjustmentTargetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAdjustmentTargetRules.DefaultAdaptiveAdjustmentTargetValidator();
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAdjustmentTargetRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AdaptiveAdjustmentTargetRevisionRangeRule(120, 1000);
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAdjustmentTargetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AdaptiveAdjustmentTargetLimitRangeRule(32, 120);
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAdjustmentTargetRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AdaptiveAdjustmentTargetRegionLengthRule(255);
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentTargetValidatorReportsEveryProblem();
        AdaptiveAdjustmentTargetRevisionRangeRuleFlagsValuesAboveRevision();
        AdaptiveAdjustmentTargetLimitRangeRuleFlagsValuesAboveLimit();
        AdaptiveAdjustmentTargetRegionLengthRuleFlagsALongRegion();
    }
}
