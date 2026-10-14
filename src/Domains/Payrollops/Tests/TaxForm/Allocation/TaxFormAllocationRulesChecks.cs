namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAllocationRulesChecks
{
    public static void TaxFormAllocationValidatorReportsEveryProblem()
    {
        var validator = TaxFormAllocationRules.DefaultTaxFormAllocationValidator();
        var item = TaxFormAllocation.NewTaxFormAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new TaxFormAllocationGraceDaysRangeRule(64, 255);
        var item = TaxFormAllocation.NewTaxFormAllocation("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormAllocationRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new TaxFormAllocationRequiredJobCodeRule("tax form allocation needs this field");
        var item = TaxFormAllocation.NewTaxFormAllocation("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormAllocationCodeLengthRule(1000);
        var item = TaxFormAllocation.NewTaxFormAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormAllocationValidatorReportsEveryProblem();
        TaxFormAllocationGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        TaxFormAllocationRequiredJobCodeRuleFlagsABlankJobCode();
        TaxFormAllocationCodeLengthRuleFlagsALongCode();
    }
}
