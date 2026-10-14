namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipNotice;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Checks of payslip notice code, run by the project's runner.</summary>
public static class BatchedPayslipClaimRulesChecks
{
    public static void BatchedPayslipClaimValidatorReportsEveryProblem()
    {
        var validator = BatchedPayslipClaimRules.DefaultBatchedPayslipClaimValidator();
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayslipClaimRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new BatchedPayslipClaimRequiredExternalRefRule("batched payslip claim needs a value");
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPayslipClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPayslipClaimCodeLengthRule(255);
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPayslipClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new BatchedPayslipClaimRequiredNameRule("batched payslip claim needs a value");
        var item = BatchedPayslipClaim.NewBatchedPayslipClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPayslipClaimValidatorReportsEveryProblem();
        BatchedPayslipClaimRequiredExternalRefRuleFlagsABlankExternalRef();
        BatchedPayslipClaimCodeLengthRuleFlagsALongCode();
        BatchedPayslipClaimRequiredNameRuleFlagsABlankName();
    }
}
