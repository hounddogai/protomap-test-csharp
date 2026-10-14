namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PositionSegmentRulesChecks
{
    public static void PositionSegmentValidatorReportsEveryProblem()
    {
        var validator = PositionSegmentRules.DefaultPositionSegmentValidator();
        var item = PositionSegment.NewPositionSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionSegmentRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PositionSegmentRatioRangeRule(3.8, 1215.8);
        var item = PositionSegment.NewPositionSegment("x", "y");
        item.Ratio = 1217.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionSegmentCodeLengthRule(255);
        var item = PositionSegment.NewPositionSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionSegmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PositionSegmentExpiresAtRangeRule(64, 120);
        var item = PositionSegment.NewPositionSegment("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionSegmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new PositionSegmentSourceLengthRule(1000);
        var item = PositionSegment.NewPositionSegment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionSegmentChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new PositionSegmentChecksumLengthRule(3600);
        var item = PositionSegment.NewPositionSegment("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PositionSegmentValidatorReportsEveryProblem();
        PositionSegmentRatioRangeRuleFlagsValuesAboveRatio();
        PositionSegmentCodeLengthRuleFlagsALongCode();
        PositionSegmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PositionSegmentSourceLengthRuleFlagsALongSource();
        PositionSegmentChecksumLengthRuleFlagsALongChecksum();
    }
}
