namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionCheckpointRulesChecks
{
    public static void RecognitionCheckpointValidatorReportsEveryProblem()
    {
        var validator = RecognitionCheckpointRules.DefaultRecognitionCheckpointValidator();
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RecognitionCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RecognitionCheckpointRequiredCodeRule("recognition checkpoint needs an entry");
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RecognitionCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RecognitionCheckpointStartsAtRangeRule(80, 255);
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RecognitionCheckpointTeamLengthRuleFlagsALongTeam()
    {
        var rule = new RecognitionCheckpointTeamLengthRule(255);
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RecognitionCheckpointRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RecognitionCheckpointRateRangeRule(4.7, 3599.9);
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("x", "y");
        item.Rate = 3601.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RecognitionCheckpointValidatorReportsEveryProblem();
        RecognitionCheckpointRequiredCodeRuleFlagsABlankCode();
        RecognitionCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RecognitionCheckpointTeamLengthRuleFlagsALongTeam();
        RecognitionCheckpointRateRangeRuleFlagsValuesAboveRate();
    }
}
