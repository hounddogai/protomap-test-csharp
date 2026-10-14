namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledTaxFormForecastRulesChecks
{
    public static void ScheduledTaxFormForecastValidatorReportsEveryProblem()
    {
        var validator = ScheduledTaxFormForecastRules.DefaultScheduledTaxFormForecastValidator();
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTaxFormForecastFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ScheduledTaxFormForecastFteRangeRule(8.4, 4133.2);
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("x", "y");
        item.Fte = 4134.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTaxFormForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledTaxFormForecastRequiredTitleRule("scheduled tax form forecast needs this field");
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledTaxFormForecastMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ScheduledTaxFormForecastMaximumRangeRule(4.8, 4375.1);
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("x", "y");
        item.Maximum = 4376.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTaxFormForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledTaxFormForecastCodeLengthRule(120);
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledTaxFormForecastValidatorReportsEveryProblem();
        ScheduledTaxFormForecastFteRangeRuleFlagsValuesAboveFte();
        ScheduledTaxFormForecastRequiredTitleRuleFlagsABlankTitle();
        ScheduledTaxFormForecastMaximumRangeRuleFlagsValuesAboveMaximum();
        ScheduledTaxFormForecastCodeLengthRuleFlagsALongCode();
    }
}
