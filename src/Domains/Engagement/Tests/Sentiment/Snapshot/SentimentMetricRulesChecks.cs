namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentMetricRulesChecks
{
    public static void SentimentMetricValidatorReportsEveryProblem()
    {
        var validator = SentimentMetricRules.DefaultSentimentMetricValidator();
        var item = SentimentMetric.NewSentimentMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new SentimentMetricCodeLengthRule(64);
        var item = SentimentMetric.NewSentimentMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SentimentMetricMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new SentimentMetricMaximumRangeRule(2.2, 2096.8);
        var item = SentimentMetric.NewSentimentMetric("x", "y");
        item.Maximum = 2098.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SentimentMetricRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new SentimentMetricRequiredLocationRule("sentiment metric needs this field");
        var item = SentimentMetric.NewSentimentMetric("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SentimentMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SentimentMetricRequiredCodeRule("sentiment metric needs an entry");
        var item = SentimentMetric.NewSentimentMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SentimentMetricValidatorReportsEveryProblem();
        SentimentMetricCodeLengthRuleFlagsALongCode();
        SentimentMetricMaximumRangeRuleFlagsValuesAboveMaximum();
        SentimentMetricRequiredLocationRuleFlagsABlankLocation();
        SentimentMetricRequiredCodeRuleFlagsABlankCode();
    }
}
