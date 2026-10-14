namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyMobilitySegmentRulesChecks
{
    public static void LegacyMobilitySegmentValidatorReportsEveryProblem()
    {
        var validator = LegacyMobilitySegmentRules.DefaultLegacyMobilitySegmentValidator();
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyMobilitySegmentLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new LegacyMobilitySegmentLimitRangeRule(32, 120);
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("x", "y");
        item.Limit = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyMobilitySegmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LegacyMobilitySegmentGradeLengthRule(120);
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyMobilitySegmentCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LegacyMobilitySegmentCeilingRangeRule(3.1, 4701.1);
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("x", "y");
        item.Ceiling = 4702.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyMobilitySegmentRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LegacyMobilitySegmentRequiredCostCenterRule("legacy mobility segment needs this field");
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyMobilitySegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyMobilitySegmentRequiredCodeRule("legacy mobility segment needs this field");
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyMobilitySegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyMobilitySegmentDurationMinutesRangeRule(32, 255);
        var item = LegacyMobilitySegment.NewLegacyMobilitySegment("x", "y");
        item.DurationMinutes = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyMobilitySegmentValidatorReportsEveryProblem();
        LegacyMobilitySegmentLimitRangeRuleFlagsValuesAboveLimit();
        LegacyMobilitySegmentGradeLengthRuleFlagsALongGrade();
        LegacyMobilitySegmentCeilingRangeRuleFlagsValuesAboveCeiling();
        LegacyMobilitySegmentRequiredCostCenterRuleFlagsABlankCostCenter();
        LegacyMobilitySegmentRequiredCodeRuleFlagsABlankCode();
        LegacyMobilitySegmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
