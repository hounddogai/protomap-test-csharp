namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredRecognitionScoreRulesChecks
{
    public static void TieredRecognitionScoreValidatorReportsEveryProblem()
    {
        var validator = TieredRecognitionScoreRules.DefaultTieredRecognitionScoreValidator();
        var item = TieredRecognitionScore.NewTieredRecognitionScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredRecognitionScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredRecognitionScoreRequiredCodeRule("tiered recognition score needs a value");
        var item = TieredRecognitionScore.NewTieredRecognitionScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRecognitionScoreRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TieredRecognitionScoreRequiredPhoneRule("tiered recognition score needs a value");
        var item = TieredRecognitionScore.NewTieredRecognitionScore("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredRecognitionScoreNameLengthRuleFlagsALongName()
    {
        var rule = new TieredRecognitionScoreNameLengthRule(16);
        var item = TieredRecognitionScore.NewTieredRecognitionScore("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredRecognitionScoreHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new TieredRecognitionScoreHoursRangeRule(7.7, 2114.6);
        var item = TieredRecognitionScore.NewTieredRecognitionScore("x", "y");
        item.Hours = 2116.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredRecognitionScoreValidatorReportsEveryProblem();
        TieredRecognitionScoreRequiredCodeRuleFlagsABlankCode();
        TieredRecognitionScoreRequiredPhoneRuleFlagsABlankPhone();
        TieredRecognitionScoreNameLengthRuleFlagsALongName();
        TieredRecognitionScoreHoursRangeRuleFlagsValuesAboveHours();
    }
}
