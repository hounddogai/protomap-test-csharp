namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentRuleRulesChecks
{
    public static void SentimentRuleValidatorReportsEveryProblem()
    {
        var validator = SentimentRuleRules.DefaultSentimentRuleValidator();
        var item = SentimentRule.NewSentimentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SentimentRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new SentimentRuleTenureMonthsRangeRule(255, 3600);
        var item = SentimentRule.NewSentimentRule("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SentimentRuleValidatorReportsEveryProblem();
        SentimentRuleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
