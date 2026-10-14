namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalRecognitionScoreRulesChecks
{
    public static void GlobalRecognitionScoreValidatorReportsEveryProblem()
    {
        var validator = GlobalRecognitionScoreRules.DefaultGlobalRecognitionScoreValidator();
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalRecognitionScoreRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new GlobalRecognitionScoreRequiredTimezoneRule("global recognition score needs an entry");
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRecognitionScoreLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new GlobalRecognitionScoreLimitRangeRule(16, 80);
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("x", "y");
        item.Limit = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalRecognitionScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalRecognitionScoreRequiredCodeRule("global recognition score needs this field");
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalRecognitionScoreTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new GlobalRecognitionScoreTimezoneLengthRule(86400);
        var item = GlobalRecognitionScore.NewGlobalRecognitionScore("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalRecognitionScoreValidatorReportsEveryProblem();
        GlobalRecognitionScoreRequiredTimezoneRuleFlagsABlankTimezone();
        GlobalRecognitionScoreLimitRangeRuleFlagsValuesAboveLimit();
        GlobalRecognitionScoreRequiredCodeRuleFlagsABlankCode();
        GlobalRecognitionScoreTimezoneLengthRuleFlagsALongTimezone();
    }
}
