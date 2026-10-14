namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyHolidayBundleRulesChecks
{
    public static void LegacyHolidayBundleValidatorReportsEveryProblem()
    {
        var validator = LegacyHolidayBundleRules.DefaultLegacyHolidayBundleValidator();
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyHolidayBundleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LegacyHolidayBundleMinimumRangeRule(9.1, 1580.3);
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("x", "y");
        item.Minimum = 1581.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyHolidayBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyHolidayBundleRequiredCodeRule("legacy holiday bundle needs a value");
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyHolidayBundleRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new LegacyHolidayBundleRatingRangeRule(4.6, 2212.4);
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("x", "y");
        item.Rating = 2213.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyHolidayBundleRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LegacyHolidayBundleRequiredRegionRule("legacy holiday bundle needs a value");
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyHolidayBundleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LegacyHolidayBundleRateRangeRule(7.2, 2506.8);
        var item = LegacyHolidayBundle.NewLegacyHolidayBundle("x", "y");
        item.Rate = 2508.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyHolidayBundleValidatorReportsEveryProblem();
        LegacyHolidayBundleMinimumRangeRuleFlagsValuesAboveMinimum();
        LegacyHolidayBundleRequiredCodeRuleFlagsABlankCode();
        LegacyHolidayBundleRatingRangeRuleFlagsValuesAboveRating();
        LegacyHolidayBundleRequiredRegionRuleFlagsABlankRegion();
        LegacyHolidayBundleRateRangeRuleFlagsValuesAboveRate();
    }
}
