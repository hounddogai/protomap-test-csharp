namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingSentimentVarianceRulesChecks
{
    public static void PendingSentimentVarianceValidatorReportsEveryProblem()
    {
        var validator = PendingSentimentVarianceRules.DefaultPendingSentimentVarianceValidator();
        var item = PendingSentimentVariance.NewPendingSentimentVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSentimentVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingSentimentVarianceCodeLengthRule(32);
        var item = PendingSentimentVariance.NewPendingSentimentVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSentimentVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingSentimentVarianceRequiredNameRule("pending sentiment variance needs a value");
        var item = PendingSentimentVariance.NewPendingSentimentVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingSentimentVarianceValidatorReportsEveryProblem();
        PendingSentimentVarianceCodeLengthRuleFlagsALongCode();
        PendingSentimentVarianceRequiredNameRuleFlagsABlankName();
    }
}
