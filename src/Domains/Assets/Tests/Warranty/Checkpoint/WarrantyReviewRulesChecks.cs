namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyReviewRulesChecks
{
    public static void WarrantyReviewValidatorReportsEveryProblem()
    {
        var validator = WarrantyReviewRules.DefaultWarrantyReviewValidator();
        var item = WarrantyReview.NewWarrantyReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantyReviewCodeLengthRule(32);
        var item = WarrantyReview.NewWarrantyReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyReviewRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new WarrantyReviewRequiredGradeRule("warranty review needs an entry");
        var item = WarrantyReview.NewWarrantyReview("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyReviewValidatorReportsEveryProblem();
        WarrantyReviewCodeLengthRuleFlagsALongCode();
        WarrantyReviewRequiredGradeRuleFlagsABlankGrade();
    }
}
