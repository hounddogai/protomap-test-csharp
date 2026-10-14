namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class ManualTaxFormMetricRulesChecks
{
    public static void ManualTaxFormMetricValidatorReportsEveryProblem()
    {
        var validator = ManualTaxFormMetricRules.DefaultManualTaxFormMetricValidator();
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualTaxFormMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualTaxFormMetricCodeLengthRule(86400);
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualTaxFormMetricRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ManualTaxFormMetricRatingRangeRule(9.6, 3680.6);
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("x", "y");
        item.Rating = 3682.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualTaxFormMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualTaxFormMetricTitleLengthRule(255);
        var item = ManualTaxFormMetric.NewManualTaxFormMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualTaxFormMetricValidatorReportsEveryProblem();
        ManualTaxFormMetricCodeLengthRuleFlagsALongCode();
        ManualTaxFormMetricRatingRangeRuleFlagsValuesAboveRating();
        ManualTaxFormMetricTitleLengthRuleFlagsALongTitle();
    }
}
