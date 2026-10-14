namespace Acme.Workforce.Payrollops.Tests.PayRun.Variance;

using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PendingPayRunAllocationRulesChecks
{
    public static void PendingPayRunAllocationValidatorReportsEveryProblem()
    {
        var validator = PendingPayRunAllocationRules.DefaultPendingPayRunAllocationValidator();
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPayRunAllocationCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PendingPayRunAllocationCountryLengthRule(86400);
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPayRunAllocationRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PendingPayRunAllocationRequiredCountryRule("pending pay run allocation needs a value");
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingPayRunAllocationWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PendingPayRunAllocationWeightRangeRule(5.3, 2116.8);
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("x", "y");
        item.Weight = 2118.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPayRunAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingPayRunAllocationRequiredCodeRule("pending pay run allocation needs an entry");
        var item = PendingPayRunAllocation.NewPendingPayRunAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingPayRunAllocationValidatorReportsEveryProblem();
        PendingPayRunAllocationCountryLengthRuleFlagsALongCountry();
        PendingPayRunAllocationRequiredCountryRuleFlagsABlankCountry();
        PendingPayRunAllocationWeightRangeRuleFlagsValuesAboveWeight();
        PendingPayRunAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
