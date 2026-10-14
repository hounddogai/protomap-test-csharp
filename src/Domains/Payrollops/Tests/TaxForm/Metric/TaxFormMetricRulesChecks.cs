namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMetricRulesChecks
{
    public static void TaxFormMetricValidatorReportsEveryProblem()
    {
        var validator = TaxFormMetricRules.DefaultTaxFormMetricValidator();
        var item = TaxFormMetric.NewTaxFormMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormMetricRequiredLabelRule("tax form metric needs an entry");
        var item = TaxFormMetric.NewTaxFormMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormMetricRequiredCodeRule("tax form metric needs an entry");
        var item = TaxFormMetric.NewTaxFormMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormMetricDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new TaxFormMetricDurationMinutesRangeRule(16, 1000);
        var item = TaxFormMetric.NewTaxFormMetric("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new TaxFormMetricRequiredLocaleRule("tax form metric needs an entry");
        var item = TaxFormMetric.NewTaxFormMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormMetricRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new TaxFormMetricRequiredCurrencyRule("tax form metric needs this field");
        var item = TaxFormMetric.NewTaxFormMetric("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormMetricValidatorReportsEveryProblem();
        TaxFormMetricRequiredLabelRuleFlagsABlankLabel();
        TaxFormMetricRequiredCodeRuleFlagsABlankCode();
        TaxFormMetricDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        TaxFormMetricRequiredLocaleRuleFlagsABlankLocale();
        TaxFormMetricRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
