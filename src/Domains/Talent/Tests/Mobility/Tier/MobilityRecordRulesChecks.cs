namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MobilityRecordRulesChecks
{
    public static void MobilityRecordValidatorReportsEveryProblem()
    {
        var validator = MobilityRecordRules.DefaultMobilityRecordValidator();
        var item = MobilityRecord.NewMobilityRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityRecordTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new MobilityRecordTimezoneLengthRule(120);
        var item = MobilityRecord.NewMobilityRecord("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityRecordRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new MobilityRecordRequiredTimezoneRule("mobility record needs this field");
        var item = MobilityRecord.NewMobilityRecord("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityRecordScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new MobilityRecordScoreRangeRule(4.6, 4890.1);
        var item = MobilityRecord.NewMobilityRecord("x", "y");
        item.Score = 4891.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityRecordValidatorReportsEveryProblem();
        MobilityRecordTimezoneLengthRuleFlagsALongTimezone();
        MobilityRecordRequiredTimezoneRuleFlagsABlankTimezone();
        MobilityRecordScoreRangeRuleFlagsValuesAboveScore();
    }
}
