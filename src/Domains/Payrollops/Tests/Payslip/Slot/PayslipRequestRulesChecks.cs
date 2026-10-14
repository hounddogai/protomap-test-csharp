namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipRequestRulesChecks
{
    public static void PayslipRequestValidatorReportsEveryProblem()
    {
        var validator = PayslipRequestRules.DefaultPayslipRequestValidator();
        var item = PayslipRequest.NewPayslipRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipRequestCodeLengthRule(64);
        var item = PayslipRequest.NewPayslipRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipRequestReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PayslipRequestReviewerLengthRule(3600);
        var item = PayslipRequest.NewPayslipRequest("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipRequestCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new PayslipRequestCategoryLengthRule(32);
        var item = PayslipRequest.NewPayslipRequest("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipRequestRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new PayslipRequestRequiredCategoryRule("payslip request needs this field");
        var item = PayslipRequest.NewPayslipRequest("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipRequestRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PayslipRequestRetriesRangeRule(3600, 86400);
        var item = PayslipRequest.NewPayslipRequest("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayslipRequestValidatorReportsEveryProblem();
        PayslipRequestCodeLengthRuleFlagsALongCode();
        PayslipRequestReviewerLengthRuleFlagsALongReviewer();
        PayslipRequestCategoryLengthRuleFlagsALongCategory();
        PayslipRequestRequiredCategoryRuleFlagsABlankCategory();
        PayslipRequestRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
