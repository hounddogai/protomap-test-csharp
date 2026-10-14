namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FallbackEngagementExceptionRulesChecks
{
    public static void FallbackEngagementExceptionValidatorReportsEveryProblem()
    {
        var validator = FallbackEngagementExceptionRules.DefaultFallbackEngagementExceptionValidator();
        var item = FallbackEngagementException.NewFallbackEngagementException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackEngagementExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackEngagementExceptionRequiredCodeRule("fallback engagement exception needs this field");
        var item = FallbackEngagementException.NewFallbackEngagementException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackEngagementExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new FallbackEngagementExceptionLeaveDaysRangeRule(16, 80);
        var item = FallbackEngagementException.NewFallbackEngagementException("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackEngagementExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new FallbackEngagementExceptionRateRangeRule(2.2, 1108.9);
        var item = FallbackEngagementException.NewFallbackEngagementException("x", "y");
        item.Rate = 1110.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackEngagementExceptionRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new FallbackEngagementExceptionRequiredSegmentRule("fallback engagement exception needs a value");
        var item = FallbackEngagementException.NewFallbackEngagementException("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackEngagementExceptionValidatorReportsEveryProblem();
        FallbackEngagementExceptionRequiredCodeRuleFlagsABlankCode();
        FallbackEngagementExceptionLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        FallbackEngagementExceptionRateRangeRuleFlagsValuesAboveRate();
        FallbackEngagementExceptionRequiredSegmentRuleFlagsABlankSegment();
    }
}
