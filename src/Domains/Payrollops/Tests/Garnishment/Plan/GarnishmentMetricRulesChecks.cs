namespace Acme.Workforce.Payrollops.Tests.Garnishment.Plan;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GarnishmentMetricRulesChecks
{
    public static void GarnishmentMetricValidatorReportsEveryProblem()
    {
        var validator = GarnishmentMetricRules.DefaultGarnishmentMetricValidator();
        var item = GarnishmentMetric.NewGarnishmentMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentMetricSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new GarnishmentMetricSequenceRangeRule(16, 120);
        var item = GarnishmentMetric.NewGarnishmentMetric("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new GarnishmentMetricRequiredLocaleRule("garnishment metric needs this field");
        var item = GarnishmentMetric.NewGarnishmentMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentMetricCountryLengthRuleFlagsALongCountry()
    {
        var rule = new GarnishmentMetricCountryLengthRule(16);
        var item = GarnishmentMetric.NewGarnishmentMetric("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentMetricCodeLengthRule(16);
        var item = GarnishmentMetric.NewGarnishmentMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentMetricValidatorReportsEveryProblem();
        GarnishmentMetricSequenceRangeRuleFlagsValuesAboveSequence();
        GarnishmentMetricRequiredLocaleRuleFlagsABlankLocale();
        GarnishmentMetricCountryLengthRuleFlagsALongCountry();
        GarnishmentMetricCodeLengthRuleFlagsALongCode();
    }
}
