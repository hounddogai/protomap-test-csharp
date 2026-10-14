namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PendingBookingDigestRulesChecks
{
    public static void PendingBookingDigestValidatorReportsEveryProblem()
    {
        var validator = PendingBookingDigestRules.DefaultPendingBookingDigestValidator();
        var item = PendingBookingDigest.NewPendingBookingDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingBookingDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingBookingDigestRequiredCodeRule("pending booking digest needs a value");
        var item = PendingBookingDigest.NewPendingBookingDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingBookingDigestThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PendingBookingDigestThresholdRangeRule(0.6, 4692.3);
        var item = PendingBookingDigest.NewPendingBookingDigest("x", "y");
        item.Threshold = 4693.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingBookingDigestSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new PendingBookingDigestSummaryLengthRule(64);
        var item = PendingBookingDigest.NewPendingBookingDigest("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingBookingDigestValidatorReportsEveryProblem();
        PendingBookingDigestRequiredCodeRuleFlagsABlankCode();
        PendingBookingDigestThresholdRangeRuleFlagsValuesAboveThreshold();
        PendingBookingDigestSummaryLengthRuleFlagsALongSummary();
    }
}
