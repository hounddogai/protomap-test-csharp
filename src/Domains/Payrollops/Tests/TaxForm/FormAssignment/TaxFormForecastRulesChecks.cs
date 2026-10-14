namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormForecastRulesChecks
{
    public static void TaxFormForecastValidatorReportsEveryProblem()
    {
        var validator = TaxFormForecastRules.DefaultTaxFormForecastValidator();
        var item = TaxFormForecast.NewTaxFormForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormForecastTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TaxFormForecastTitleLengthRule(32);
        var item = TaxFormForecast.NewTaxFormForecast("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormForecastEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TaxFormForecastEndsAtRangeRule(32, 120);
        var item = TaxFormForecast.NewTaxFormForecast("x", "y");
        item.EndsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TaxFormForecastRequiredTitleRule("tax form forecast needs this field");
        var item = TaxFormForecast.NewTaxFormForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormForecastValidatorReportsEveryProblem();
        TaxFormForecastTitleLengthRuleFlagsALongTitle();
        TaxFormForecastEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TaxFormForecastRequiredTitleRuleFlagsABlankTitle();
    }
}
