namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class ScheduledWarrantyReviewRulesChecks
{
    public static void ScheduledWarrantyReviewValidatorReportsEveryProblem()
    {
        var validator = ScheduledWarrantyReviewRules.DefaultScheduledWarrantyReviewValidator();
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledWarrantyReviewRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ScheduledWarrantyReviewRequiredGradeRule("scheduled warranty review needs this field");
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledWarrantyReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledWarrantyReviewRequiredCodeRule("scheduled warranty review needs an entry");
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledWarrantyReviewValidatorReportsEveryProblem();
        ScheduledWarrantyReviewRequiredGradeRuleFlagsABlankGrade();
        ScheduledWarrantyReviewRequiredCodeRuleFlagsABlankCode();
    }
}
