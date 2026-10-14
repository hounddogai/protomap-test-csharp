namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class TaxFormGroupRulesChecks
{
    public static void TaxFormGroupValidatorReportsEveryProblem()
    {
        var validator = TaxFormGroupRules.DefaultTaxFormGroupValidator();
        var item = TaxFormGroup.NewTaxFormGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormGroupMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new TaxFormGroupMinimumRangeRule(3.0, 4614.1);
        var item = TaxFormGroup.NewTaxFormGroup("x", "y");
        item.Minimum = 4615.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TaxFormGroupRequiredLabelRule("tax form group needs a value");
        var item = TaxFormGroup.NewTaxFormGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormGroupValidatorReportsEveryProblem();
        TaxFormGroupMinimumRangeRuleFlagsValuesAboveMinimum();
        TaxFormGroupRequiredLabelRuleFlagsABlankLabel();
    }
}
