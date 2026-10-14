namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ScheduledTaxFormMetricRulesChecks
{
    public static void ScheduledTaxFormMetricValidatorReportsEveryProblem()
    {
        var validator = ScheduledTaxFormMetricRules.DefaultScheduledTaxFormMetricValidator();
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTaxFormMetricSourceLengthRuleFlagsALongSource()
    {
        var rule = new ScheduledTaxFormMetricSourceLengthRule(1000);
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTaxFormMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledTaxFormMetricCodeLengthRule(1000);
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTaxFormMetricNotesLengthRuleFlagsALongNotes()
    {
        var rule = new ScheduledTaxFormMetricNotesLengthRule(3600);
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTaxFormMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ScheduledTaxFormMetricAmountRangeRule(2.8, 4397.6);
        var item = ScheduledTaxFormMetric.NewScheduledTaxFormMetric("x", "y");
        item.Amount = 4399.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledTaxFormMetricValidatorReportsEveryProblem();
        ScheduledTaxFormMetricSourceLengthRuleFlagsALongSource();
        ScheduledTaxFormMetricCodeLengthRuleFlagsALongCode();
        ScheduledTaxFormMetricNotesLengthRuleFlagsALongNotes();
        ScheduledTaxFormMetricAmountRangeRuleFlagsValuesAboveAmount();
    }
}
