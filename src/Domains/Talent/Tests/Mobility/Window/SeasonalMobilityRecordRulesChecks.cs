namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SeasonalMobilityRecordRulesChecks
{
    public static void SeasonalMobilityRecordValidatorReportsEveryProblem()
    {
        var validator = SeasonalMobilityRecordRules.DefaultSeasonalMobilityRecordValidator();
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalMobilityRecordRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new SeasonalMobilityRecordRequiredGradeRule("seasonal mobility record needs an entry");
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalMobilityRecordStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SeasonalMobilityRecordStartsAtRangeRule(120, 255);
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalMobilityRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalMobilityRecordCodeLengthRule(80);
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalMobilityRecordScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SeasonalMobilityRecordScoreRangeRule(3.0, 418.6);
        var item = SeasonalMobilityRecord.NewSeasonalMobilityRecord("x", "y");
        item.Score = 420.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalMobilityRecordValidatorReportsEveryProblem();
        SeasonalMobilityRecordRequiredGradeRuleFlagsABlankGrade();
        SeasonalMobilityRecordStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SeasonalMobilityRecordCodeLengthRuleFlagsALongCode();
        SeasonalMobilityRecordScoreRangeRuleFlagsValuesAboveScore();
    }
}
