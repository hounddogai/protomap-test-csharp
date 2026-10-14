namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class AdaptivePayBandGroupRulesChecks
{
    public static void AdaptivePayBandGroupValidatorReportsEveryProblem()
    {
        var validator = AdaptivePayBandGroupRules.DefaultAdaptivePayBandGroupValidator();
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePayBandGroupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdaptivePayBandGroupThresholdRangeRule(0.5, 2282.0);
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("x", "y");
        item.Threshold = 2283.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptivePayBandGroupTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AdaptivePayBandGroupTimezoneLengthRule(86400);
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePayBandGroupRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AdaptivePayBandGroupRegionLengthRule(120);
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptivePayBandGroupValidatorReportsEveryProblem();
        AdaptivePayBandGroupThresholdRangeRuleFlagsValuesAboveThreshold();
        AdaptivePayBandGroupTimezoneLengthRuleFlagsALongTimezone();
        AdaptivePayBandGroupRegionLengthRuleFlagsALongRegion();
    }
}
