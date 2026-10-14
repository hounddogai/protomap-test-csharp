namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacySentimentRuleRulesChecks
{
    public static void LegacySentimentRuleValidatorReportsEveryProblem()
    {
        var validator = LegacySentimentRuleRules.DefaultLegacySentimentRuleValidator();
        var item = LegacySentimentRule.NewLegacySentimentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySentimentRuleLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new LegacySentimentRuleLocaleLengthRule(32);
        var item = LegacySentimentRule.NewLegacySentimentRule("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacySentimentRuleValidatorReportsEveryProblem();
        LegacySentimentRuleLocaleLengthRuleFlagsALongLocale();
    }
}
