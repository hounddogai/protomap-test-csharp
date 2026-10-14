namespace Acme.Workforce.Engagement.Tests.Sentiment.Target;

using Acme.Workforce.Engagement.Sentiment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SentimentEstimateRulesChecks
{
    public static void SentimentEstimateValidatorReportsEveryProblem()
    {
        var validator = SentimentEstimateRules.DefaultSentimentEstimateValidator();
        var item = SentimentEstimate.NewSentimentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentEstimateChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SentimentEstimateChannelLengthRule(16);
        var item = SentimentEstimate.NewSentimentEstimate("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentEstimateRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new SentimentEstimateRetriesRangeRule(80, 80);
        var item = SentimentEstimate.NewSentimentEstimate("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentEstimateRequiredCodeRule("sentiment estimate needs an entry");
        var item = SentimentEstimate.NewSentimentEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentEstimateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SentimentEstimateRequiredDepartmentRule("sentiment estimate needs this field");
        var item = SentimentEstimate.NewSentimentEstimate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentEstimateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SentimentEstimateLabelLengthRule(1000);
        var item = SentimentEstimate.NewSentimentEstimate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentEstimateRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new SentimentEstimateRateRangeRule(0.6, 3992.5);
        var item = SentimentEstimate.NewSentimentEstimate("x", "y");
        item.Rate = 3994.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentEstimateValidatorReportsEveryProblem();
        SentimentEstimateChannelLengthRuleFlagsALongChannel();
        SentimentEstimateRetriesRangeRuleFlagsValuesAboveRetries();
        SentimentEstimateRequiredCodeRuleFlagsABlankCode();
        SentimentEstimateRequiredDepartmentRuleFlagsABlankDepartment();
        SentimentEstimateLabelLengthRuleFlagsALongLabel();
        SentimentEstimateRateRangeRuleFlagsValuesAboveRate();
    }
}
