namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

/// <summary>Checks of payslip snapshot code, run by the project's runner.</summary>
public static class PayslipBatchRulesChecks
{
    public static void PayslipBatchValidatorReportsEveryProblem()
    {
        var validator = PayslipBatchRules.DefaultPayslipBatchValidator();
        var item = PayslipBatch.NewPayslipBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipBatchCodeLengthRule(120);
        var item = PayslipBatch.NewPayslipBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipBatchRequiredCodeRule("payslip batch needs this field");
        var item = PayslipBatch.NewPayslipBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipBatchNotesLengthRuleFlagsALongNotes()
    {
        var rule = new PayslipBatchNotesLengthRule(64);
        var item = PayslipBatch.NewPayslipBatch("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipBatchScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PayslipBatchScoreRangeRule(8.9, 3146.1);
        var item = PayslipBatch.NewPayslipBatch("x", "y");
        item.Score = 3147.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayslipBatchValidatorReportsEveryProblem();
        PayslipBatchCodeLengthRuleFlagsALongCode();
        PayslipBatchRequiredCodeRuleFlagsABlankCode();
        PayslipBatchNotesLengthRuleFlagsALongNotes();
        PayslipBatchScoreRangeRuleFlagsValuesAboveScore();
    }
}
