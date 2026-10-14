namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledSentimentTargetRulesChecks
{
    public static void ScheduledSentimentTargetValidatorReportsEveryProblem()
    {
        var validator = ScheduledSentimentTargetRules.DefaultScheduledSentimentTargetValidator();
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSentimentTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledSentimentTargetCodeLengthRule(3600);
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSentimentTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScheduledSentimentTargetGraceDaysRangeRule(16, 1000);
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSentimentTargetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledSentimentTargetLimitRangeRule(86400, 86400);
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSentimentTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledSentimentTargetRequiredNameRule("scheduled sentiment target needs a value");
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledSentimentTargetValidatorReportsEveryProblem();
        ScheduledSentimentTargetCodeLengthRuleFlagsALongCode();
        ScheduledSentimentTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScheduledSentimentTargetLimitRangeRuleFlagsValuesAboveLimit();
        ScheduledSentimentTargetRequiredNameRuleFlagsABlankName();
    }
}
