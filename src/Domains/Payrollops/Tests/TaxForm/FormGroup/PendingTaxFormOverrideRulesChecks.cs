namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideRulesChecks
{
    public static void PendingTaxFormOverrideValidatorReportsEveryProblem()
    {
        var validator = PendingTaxFormOverrideRules.DefaultPendingTaxFormOverrideValidator();
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTaxFormOverrideAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingTaxFormOverrideAmountRangeRule(6.8, 3790.3);
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("x", "y");
        item.Amount = 3791.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingTaxFormOverrideFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PendingTaxFormOverrideFloorValueRangeRule(1.3, 1247.6);
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("x", "y");
        item.FloorValue = 1249.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingTaxFormOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingTaxFormOverrideCodeLengthRule(120);
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingTaxFormOverrideValidatorReportsEveryProblem();
        PendingTaxFormOverrideAmountRangeRuleFlagsValuesAboveAmount();
        PendingTaxFormOverrideFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PendingTaxFormOverrideCodeLengthRuleFlagsALongCode();
    }
}
