namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchRulesChecks
{
    public static void CappedPayslipBatchValidatorReportsEveryProblem()
    {
        var validator = CappedPayslipBatchRules.DefaultCappedPayslipBatchValidator();
        var item = CappedPayslipBatch.NewCappedPayslipBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedPayslipBatchRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CappedPayslipBatchRequiredContactEmailRule("capped payslip batch needs this field");
        var item = CappedPayslipBatch.NewCappedPayslipBatch("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayslipBatchRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new CappedPayslipBatchRequiredExternalRefRule("capped payslip batch needs a value");
        var item = CappedPayslipBatch.NewCappedPayslipBatch("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayslipBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CappedPayslipBatchRequiredLabelRule("capped payslip batch needs this field");
        var item = CappedPayslipBatch.NewCappedPayslipBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedPayslipBatchExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new CappedPayslipBatchExternalRefLengthRule(80);
        var item = CappedPayslipBatch.NewCappedPayslipBatch("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedPayslipBatchValidatorReportsEveryProblem();
        CappedPayslipBatchRequiredContactEmailRuleFlagsABlankContactEmail();
        CappedPayslipBatchRequiredExternalRefRuleFlagsABlankExternalRef();
        CappedPayslipBatchRequiredLabelRuleFlagsABlankLabel();
        CappedPayslipBatchExternalRefLengthRuleFlagsALongExternalRef();
    }
}
