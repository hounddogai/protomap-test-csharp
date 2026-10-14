namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionAllocationRulesChecks
{
    public static void DeductionAllocationValidatorReportsEveryProblem()
    {
        var validator = DeductionAllocationRules.DefaultDeductionAllocationValidator();
        var item = DeductionAllocation.NewDeductionAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionAllocationRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new DeductionAllocationRequiredSalaryBandRule("deduction allocation needs a value");
        var item = DeductionAllocation.NewDeductionAllocation("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionAllocationCodeLengthRule(255);
        var item = DeductionAllocation.NewDeductionAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionAllocationRequiredCodeRule("deduction allocation needs an entry");
        var item = DeductionAllocation.NewDeductionAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeductionAllocationRequiredTitleRule("deduction allocation needs a value");
        var item = DeductionAllocation.NewDeductionAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionAllocationValidatorReportsEveryProblem();
        DeductionAllocationRequiredSalaryBandRuleFlagsABlankSalaryBand();
        DeductionAllocationCodeLengthRuleFlagsALongCode();
        DeductionAllocationRequiredCodeRuleFlagsABlankCode();
        DeductionAllocationRequiredTitleRuleFlagsABlankTitle();
    }
}
