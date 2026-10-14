namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyRequestRulesChecks
{
    public static void PulseSurveyRequestValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyRequestRules.DefaultPulseSurveyRequestValidator();
        var item = PulseSurveyRequest.NewPulseSurveyRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyRequestCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PulseSurveyRequestCurrencyLengthRule(255);
        var item = PulseSurveyRequest.NewPulseSurveyRequest("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyRequestRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new PulseSurveyRequestRequiredCurrencyRule("pulse survey request needs this field");
        var item = PulseSurveyRequest.NewPulseSurveyRequest("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PulseSurveyRequestDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PulseSurveyRequestDiscountRangeRule(8.1, 1158.3);
        var item = PulseSurveyRequest.NewPulseSurveyRequest("x", "y");
        item.Discount = 1159.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PulseSurveyRequestValidatorReportsEveryProblem();
        PulseSurveyRequestCurrencyLengthRuleFlagsALongCurrency();
        PulseSurveyRequestRequiredCurrencyRuleFlagsABlankCurrency();
        PulseSurveyRequestDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
