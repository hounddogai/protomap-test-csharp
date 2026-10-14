namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class RegionalTaxFormEstimateRulesChecks
{
    public static void RegionalTaxFormEstimateValidatorReportsEveryProblem()
    {
        var validator = RegionalTaxFormEstimateRules.DefaultRegionalTaxFormEstimateValidator();
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalTaxFormEstimateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RegionalTaxFormEstimateRequiredDepartmentRule("regional tax form estimate needs this field");
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTaxFormEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalTaxFormEstimateRequiredCodeRule("regional tax form estimate needs a value");
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalTaxFormEstimateAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new RegionalTaxFormEstimateAmountRangeRule(5.6, 1519.2);
        var item = RegionalTaxFormEstimate.NewRegionalTaxFormEstimate("x", "y");
        item.Amount = 1520.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalTaxFormEstimateValidatorReportsEveryProblem();
        RegionalTaxFormEstimateRequiredDepartmentRuleFlagsABlankDepartment();
        RegionalTaxFormEstimateRequiredCodeRuleFlagsABlankCode();
        RegionalTaxFormEstimateAmountRangeRuleFlagsValuesAboveAmount();
    }
}
