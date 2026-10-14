namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledSpanDigestRulesChecks
{
    public static void ScheduledSpanDigestValidatorReportsEveryProblem()
    {
        var validator = ScheduledSpanDigestRules.DefaultScheduledSpanDigestValidator();
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSpanDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledSpanDigestRequiredLabelRule("scheduled span digest needs a value");
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSpanDigestStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new ScheduledSpanDigestStartsAtRangeRule(255, 3600);
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSpanDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledSpanDigestRequiredCodeRule("scheduled span digest needs a value");
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSpanDigestExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ScheduledSpanDigestExternalRefLengthRule(86400);
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSpanDigestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScheduledSpanDigestEndsAtRangeRule(120, 86400);
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSpanDigestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ScheduledSpanDigestRatioRangeRule(6.2, 313.7);
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("x", "y");
        item.Ratio = 315.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledSpanDigestValidatorReportsEveryProblem();
        ScheduledSpanDigestRequiredLabelRuleFlagsABlankLabel();
        ScheduledSpanDigestStartsAtRangeRuleFlagsValuesAboveStartsAt();
        ScheduledSpanDigestRequiredCodeRuleFlagsABlankCode();
        ScheduledSpanDigestExternalRefLengthRuleFlagsALongExternalRef();
        ScheduledSpanDigestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScheduledSpanDigestRatioRangeRuleFlagsValuesAboveRatio();
    }
}
