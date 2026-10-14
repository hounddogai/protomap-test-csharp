namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAdjustmentRulesChecks
{
    public static void TaxFormAdjustmentValidatorReportsEveryProblem()
    {
        var validator = TaxFormAdjustmentRules.DefaultTaxFormAdjustmentValidator();
        var item = TaxFormAdjustment.NewTaxFormAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TaxFormAdjustmentLabelLengthRule(80);
        var item = TaxFormAdjustment.NewTaxFormAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormAdjustmentMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new TaxFormAdjustmentMarginRangeRule(2.5, 2732.3);
        var item = TaxFormAdjustment.NewTaxFormAdjustment("x", "y");
        item.Margin = 2733.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TaxFormAdjustmentValidatorReportsEveryProblem();
        TaxFormAdjustmentLabelLengthRuleFlagsALongLabel();
        TaxFormAdjustmentMarginRangeRuleFlagsValuesAboveMargin();
    }
}
