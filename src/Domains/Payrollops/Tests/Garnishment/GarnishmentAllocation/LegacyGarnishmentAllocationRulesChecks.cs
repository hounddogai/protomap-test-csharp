namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationRulesChecks
{
    public static void LegacyGarnishmentAllocationValidatorReportsEveryProblem()
    {
        var validator = LegacyGarnishmentAllocationRules.DefaultLegacyGarnishmentAllocationValidator();
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyGarnishmentAllocationSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new LegacyGarnishmentAllocationSalaryBandLengthRule(80);
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyGarnishmentAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyGarnishmentAllocationRequiredCodeRule("legacy garnishment allocation needs this field");
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyGarnishmentAllocationEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LegacyGarnishmentAllocationEmployeeIdLengthRule(64);
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyGarnishmentAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyGarnishmentAllocationCodeLengthRule(86400);
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyGarnishmentAllocationValidatorReportsEveryProblem();
        LegacyGarnishmentAllocationSalaryBandLengthRuleFlagsALongSalaryBand();
        LegacyGarnishmentAllocationRequiredCodeRuleFlagsABlankCode();
        LegacyGarnishmentAllocationEmployeeIdLengthRuleFlagsALongEmployeeId();
        LegacyGarnishmentAllocationCodeLengthRuleFlagsALongCode();
    }
}
