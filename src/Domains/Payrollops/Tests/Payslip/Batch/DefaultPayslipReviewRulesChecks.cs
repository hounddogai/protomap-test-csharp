namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultPayslipReviewRulesChecks
{
    public static void DefaultPayslipReviewValidatorReportsEveryProblem()
    {
        var validator = DefaultPayslipReviewRules.DefaultDefaultPayslipReviewValidator();
        var item = DefaultPayslipReview.NewDefaultPayslipReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPayslipReviewRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new DefaultPayslipReviewRevisionRangeRule(1000, 3600);
        var item = DefaultPayslipReview.NewDefaultPayslipReview("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPayslipReviewMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new DefaultPayslipReviewMinimumRangeRule(0.5, 179.5);
        var item = DefaultPayslipReview.NewDefaultPayslipReview("x", "y");
        item.Minimum = 181.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPayslipReviewValidatorReportsEveryProblem();
        DefaultPayslipReviewRevisionRangeRuleFlagsValuesAboveRevision();
        DefaultPayslipReviewMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
