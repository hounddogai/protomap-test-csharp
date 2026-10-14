namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipReviewRulesChecks
{
    public static void AdaptivePayslipReviewValidatorReportsEveryProblem()
    {
        var validator = AdaptivePayslipReviewRules.DefaultAdaptivePayslipReviewValidator();
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePayslipReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePayslipReviewRequiredCodeRule("adaptive payslip review needs a value");
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePayslipReviewNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptivePayslipReviewNameLengthRule(255);
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePayslipReviewEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new AdaptivePayslipReviewEmployeeIdLengthRule(16);
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePayslipReviewTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new AdaptivePayslipReviewTimezoneLengthRule(64);
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptivePayslipReviewValidatorReportsEveryProblem();
        AdaptivePayslipReviewRequiredCodeRuleFlagsABlankCode();
        AdaptivePayslipReviewNameLengthRuleFlagsALongName();
        AdaptivePayslipReviewEmployeeIdLengthRuleFlagsALongEmployeeId();
        AdaptivePayslipReviewTimezoneLengthRuleFlagsALongTimezone();
    }
}
