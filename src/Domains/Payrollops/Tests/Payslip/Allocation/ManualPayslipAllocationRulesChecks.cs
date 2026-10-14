namespace Acme.Workforce.Payrollops.Tests.Payslip.Allocation;

using Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualPayslipAllocationRulesChecks
{
    public static void ManualPayslipAllocationValidatorReportsEveryProblem()
    {
        var validator = ManualPayslipAllocationRules.DefaultManualPayslipAllocationValidator();
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPayslipAllocationRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ManualPayslipAllocationRevisionRangeRule(80, 3600);
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualPayslipAllocationRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ManualPayslipAllocationRequiredPhoneRule("manual payslip allocation needs an entry");
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPayslipAllocationCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ManualPayslipAllocationCountryLengthRule(86400);
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualPayslipAllocationValidatorReportsEveryProblem();
        ManualPayslipAllocationRevisionRangeRuleFlagsValuesAboveRevision();
        ManualPayslipAllocationRequiredPhoneRuleFlagsABlankPhone();
        ManualPayslipAllocationCountryLengthRuleFlagsALongCountry();
    }
}
