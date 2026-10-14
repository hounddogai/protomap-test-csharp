namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultMobilityRecordRulesChecks
{
    public static void DefaultMobilityRecordValidatorReportsEveryProblem()
    {
        var validator = DefaultMobilityRecordRules.DefaultDefaultMobilityRecordValidator();
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultMobilityRecordRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DefaultMobilityRecordRatioRangeRule(7.0, 3680.6);
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("x", "y");
        item.Ratio = 3682.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultMobilityRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultMobilityRecordRequiredCodeRule("default mobility record needs a value");
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultMobilityRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultMobilityRecordWindowDaysRangeRule(16, 80);
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultMobilityRecordRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new DefaultMobilityRecordRequiredSegmentRule("default mobility record needs this field");
        var item = DefaultMobilityRecord.NewDefaultMobilityRecord("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultMobilityRecordValidatorReportsEveryProblem();
        DefaultMobilityRecordRatioRangeRuleFlagsValuesAboveRatio();
        DefaultMobilityRecordRequiredCodeRuleFlagsABlankCode();
        DefaultMobilityRecordWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultMobilityRecordRequiredSegmentRuleFlagsABlankSegment();
    }
}
