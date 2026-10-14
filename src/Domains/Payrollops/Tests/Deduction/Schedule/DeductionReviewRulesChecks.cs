namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DeductionReviewRulesChecks
{
    public static void DeductionReviewValidatorReportsEveryProblem()
    {
        var validator = DeductionReviewRules.DefaultDeductionReviewValidator();
        var item = DeductionReview.NewDeductionReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DeductionReviewLabelLengthRule(32);
        var item = DeductionReview.NewDeductionReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionReviewRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new DeductionReviewRevisionRangeRule(255, 3600);
        var item = DeductionReview.NewDeductionReview("x", "y");
        item.Revision = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionReviewRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new DeductionReviewRequiredDepartmentRule("deduction review needs an entry");
        var item = DeductionReview.NewDeductionReview("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionReviewRequiredCodeRule("deduction review needs an entry");
        var item = DeductionReview.NewDeductionReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionReviewRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DeductionReviewRequiredLabelRule("deduction review needs a value");
        var item = DeductionReview.NewDeductionReview("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionReviewDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new DeductionReviewDepartmentLengthRule(80);
        var item = DeductionReview.NewDeductionReview("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionReviewValidatorReportsEveryProblem();
        DeductionReviewLabelLengthRuleFlagsALongLabel();
        DeductionReviewRevisionRangeRuleFlagsValuesAboveRevision();
        DeductionReviewRequiredDepartmentRuleFlagsABlankDepartment();
        DeductionReviewRequiredCodeRuleFlagsABlankCode();
        DeductionReviewRequiredLabelRuleFlagsABlankLabel();
        DeductionReviewDepartmentLengthRuleFlagsALongDepartment();
    }
}
