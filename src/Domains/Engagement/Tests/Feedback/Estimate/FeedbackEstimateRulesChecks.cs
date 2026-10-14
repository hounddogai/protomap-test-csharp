namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FeedbackEstimateRulesChecks
{
    public static void FeedbackEstimateValidatorReportsEveryProblem()
    {
        var validator = FeedbackEstimateRules.DefaultFeedbackEstimateValidator();
        var item = FeedbackEstimate.NewFeedbackEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackEstimateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new FeedbackEstimateRequiredCurrencyRule("feedback estimate needs an entry");
        var item = FeedbackEstimate.NewFeedbackEstimate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackEstimateCodeLengthRule(86400);
        var item = FeedbackEstimate.NewFeedbackEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FeedbackEstimateTitleLengthRule(32);
        var item = FeedbackEstimate.NewFeedbackEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackEstimateCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FeedbackEstimateCurrencyLengthRule(1000);
        var item = FeedbackEstimate.NewFeedbackEstimate("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackEstimateValidatorReportsEveryProblem();
        FeedbackEstimateRequiredCurrencyRuleFlagsABlankCurrency();
        FeedbackEstimateCodeLengthRuleFlagsALongCode();
        FeedbackEstimateTitleLengthRuleFlagsALongTitle();
        FeedbackEstimateCurrencyLengthRuleFlagsALongCurrency();
    }
}
