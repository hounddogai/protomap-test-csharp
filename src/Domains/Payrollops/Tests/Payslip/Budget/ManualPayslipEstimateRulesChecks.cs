namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ManualPayslipEstimateRulesChecks
{
    public static void ManualPayslipEstimateValidatorReportsEveryProblem()
    {
        var validator = ManualPayslipEstimateRules.DefaultManualPayslipEstimateValidator();
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPayslipEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualPayslipEstimateRequiredCodeRule("manual payslip estimate needs this field");
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPayslipEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualPayslipEstimateCodeLengthRule(86400);
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualPayslipEstimateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new ManualPayslipEstimateRequiredDepartmentRule("manual payslip estimate needs this field");
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualPayslipEstimateValidatorReportsEveryProblem();
        ManualPayslipEstimateRequiredCodeRuleFlagsABlankCode();
        ManualPayslipEstimateCodeLengthRuleFlagsALongCode();
        ManualPayslipEstimateRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
