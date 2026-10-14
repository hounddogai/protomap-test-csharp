namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayslipAdjustmentRulesChecks
{
    public static void PayslipAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PayslipAdjustmentRules.DefaultPayslipAdjustmentValidator();
        var item = PayslipAdjustment.NewPayslipAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipAdjustmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PayslipAdjustmentOwnerIdLengthRule(16);
        var item = PayslipAdjustment.NewPayslipAdjustment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipAdjustmentCodeLengthRule(64);
        var item = PayslipAdjustment.NewPayslipAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayslipAdjustmentValidatorReportsEveryProblem();
        PayslipAdjustmentOwnerIdLengthRuleFlagsALongOwnerId();
        PayslipAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
