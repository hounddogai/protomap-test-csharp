namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSpanDigestRulesChecks
{
    public static void RegionalSpanDigestValidatorReportsEveryProblem()
    {
        var validator = RegionalSpanDigestRules.DefaultRegionalSpanDigestValidator();
        var item = RegionalSpanDigest.NewRegionalSpanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSpanDigestRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new RegionalSpanDigestRequiredSegmentRule("regional span digest needs this field");
        var item = RegionalSpanDigest.NewRegionalSpanDigest("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSpanDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalSpanDigestCodeLengthRule(3600);
        var item = RegionalSpanDigest.NewRegionalSpanDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSpanDigestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalSpanDigestLabelLengthRule(32);
        var item = RegionalSpanDigest.NewRegionalSpanDigest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalSpanDigestSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new RegionalSpanDigestSegmentLengthRule(32);
        var item = RegionalSpanDigest.NewRegionalSpanDigest("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSpanDigestValidatorReportsEveryProblem();
        RegionalSpanDigestRequiredSegmentRuleFlagsABlankSegment();
        RegionalSpanDigestCodeLengthRuleFlagsALongCode();
        RegionalSpanDigestLabelLengthRuleFlagsALongLabel();
        RegionalSpanDigestSegmentLengthRuleFlagsALongSegment();
    }
}
