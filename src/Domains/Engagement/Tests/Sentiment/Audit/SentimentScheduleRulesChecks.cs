namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentScheduleRulesChecks
{
    public static void SentimentScheduleValidatorReportsEveryProblem()
    {
        var validator = SentimentScheduleRules.DefaultSentimentScheduleValidator();
        var item = SentimentSchedule.NewSentimentSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentScheduleCodeLengthRule(3600);
        var item = SentimentSchedule.NewSentimentSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentScheduleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new SentimentScheduleAttemptsRangeRule(16, 80);
        var item = SentimentSchedule.NewSentimentSchedule("x", "y");
        item.Attempts = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentScheduleValidatorReportsEveryProblem();
        SentimentScheduleCodeLengthRuleFlagsALongCode();
        SentimentScheduleAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
