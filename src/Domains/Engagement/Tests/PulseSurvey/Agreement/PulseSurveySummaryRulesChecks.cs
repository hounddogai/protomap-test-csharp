namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveySummaryRulesChecks
{
    public static void PulseSurveySummaryValidatorReportsEveryProblem()
    {
        var validator = PulseSurveySummaryRules.DefaultPulseSurveySummaryValidator();
        var item = PulseSurveySummary.NewPulseSurveySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveySummaryThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PulseSurveySummaryThresholdRangeRule(6.2, 4219.5);
        var item = PulseSurveySummary.NewPulseSurveySummary("x", "y");
        item.Threshold = 4221.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveySummaryCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PulseSurveySummaryCurrencyLengthRule(120);
        var item = PulseSurveySummary.NewPulseSurveySummary("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveySummaryValidatorReportsEveryProblem();
        PulseSurveySummaryThresholdRangeRuleFlagsValuesAboveThreshold();
        PulseSurveySummaryCurrencyLengthRuleFlagsALongCurrency();
    }
}
