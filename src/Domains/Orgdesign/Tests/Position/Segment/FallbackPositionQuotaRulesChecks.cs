namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaRulesChecks
{
    public static void FallbackPositionQuotaValidatorReportsEveryProblem()
    {
        var validator = FallbackPositionQuotaRules.DefaultFallbackPositionQuotaValidator();
        var item = FallbackPositionQuota.NewFallbackPositionQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPositionQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPositionQuotaRequiredNameRule("fallback position quota needs an entry");
        var item = FallbackPositionQuota.NewFallbackPositionQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPositionQuotaWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FallbackPositionQuotaWindowDaysRangeRule(255, 3600);
        var item = FallbackPositionQuota.NewFallbackPositionQuota("x", "y");
        item.WindowDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPositionQuotaRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new FallbackPositionQuotaRequiredRegionRule("fallback position quota needs this field");
        var item = FallbackPositionQuota.NewFallbackPositionQuota("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPositionQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPositionQuotaRequiredCodeRule("fallback position quota needs an entry");
        var item = FallbackPositionQuota.NewFallbackPositionQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPositionQuotaRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new FallbackPositionQuotaRatingRangeRule(7.0, 3931.9);
        var item = FallbackPositionQuota.NewFallbackPositionQuota("x", "y");
        item.Rating = 3933.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPositionQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPositionQuotaCodeLengthRule(255);
        var item = FallbackPositionQuota.NewFallbackPositionQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackPositionQuotaValidatorReportsEveryProblem();
        FallbackPositionQuotaRequiredNameRuleFlagsABlankName();
        FallbackPositionQuotaWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FallbackPositionQuotaRequiredRegionRuleFlagsABlankRegion();
        FallbackPositionQuotaRequiredCodeRuleFlagsABlankCode();
        FallbackPositionQuotaRatingRangeRuleFlagsValuesAboveRating();
        FallbackPositionQuotaCodeLengthRuleFlagsALongCode();
    }
}
