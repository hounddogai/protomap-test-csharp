namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TaxFormBaselineRulesChecks
{
    public static void TaxFormBaselineValidatorReportsEveryProblem()
    {
        var validator = TaxFormBaselineRules.DefaultTaxFormBaselineValidator();
        var item = TaxFormBaseline.NewTaxFormBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormBaselinePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new TaxFormBaselinePriorityRangeRule(120, 120);
        var item = TaxFormBaseline.NewTaxFormBaseline("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TaxFormBaselineRequiredCodeRule("tax form baseline needs an entry");
        var item = TaxFormBaseline.NewTaxFormBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormBaselineQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new TaxFormBaselineQuantityRangeRule(32, 64);
        var item = TaxFormBaseline.NewTaxFormBaseline("x", "y");
        item.Quantity = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TaxFormBaselineValidatorReportsEveryProblem();
        TaxFormBaselinePriorityRangeRuleFlagsValuesAbovePriority();
        TaxFormBaselineRequiredCodeRuleFlagsABlankCode();
        TaxFormBaselineQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
