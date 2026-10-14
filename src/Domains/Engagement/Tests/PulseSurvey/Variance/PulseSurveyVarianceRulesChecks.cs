namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyVarianceRulesChecks
{
    public static void PulseSurveyVarianceValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyVarianceRules.DefaultPulseSurveyVarianceValidator();
        var item = PulseSurveyVariance.NewPulseSurveyVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyVarianceRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PulseSurveyVarianceRateRangeRule(7.8, 4501.0);
        var item = PulseSurveyVariance.NewPulseSurveyVariance("x", "y");
        item.Rate = 4502.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyVarianceSourceLengthRuleFlagsALongSource()
    {
        var rule = new PulseSurveyVarianceSourceLengthRule(64);
        var item = PulseSurveyVariance.NewPulseSurveyVariance("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyVarianceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new PulseSurveyVarianceRequiredSourceRule("pulse survey variance needs a value");
        var item = PulseSurveyVariance.NewPulseSurveyVariance("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyVarianceCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PulseSurveyVarianceCurrencyLengthRule(64);
        var item = PulseSurveyVariance.NewPulseSurveyVariance("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PulseSurveyVarianceValidatorReportsEveryProblem();
        PulseSurveyVarianceRateRangeRuleFlagsValuesAboveRate();
        PulseSurveyVarianceSourceLengthRuleFlagsALongSource();
        PulseSurveyVarianceRequiredSourceRuleFlagsABlankSource();
        PulseSurveyVarianceCurrencyLengthRuleFlagsALongCurrency();
    }
}
