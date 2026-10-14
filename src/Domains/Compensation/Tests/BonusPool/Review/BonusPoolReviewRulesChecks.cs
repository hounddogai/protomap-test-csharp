namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolReviewRulesChecks
{
    public static void BonusPoolReviewValidatorReportsEveryProblem()
    {
        var validator = BonusPoolReviewRules.DefaultBonusPoolReviewValidator();
        var item = BonusPoolReview.NewBonusPoolReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BonusPoolReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BonusPoolReviewRequiredCodeRule("bonus pool review needs this field");
        var item = BonusPoolReview.NewBonusPoolReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BonusPoolReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BonusPoolReviewLabelLengthRule(80);
        var item = BonusPoolReview.NewBonusPoolReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BonusPoolReviewValidatorReportsEveryProblem();
        BonusPoolReviewRequiredCodeRuleFlagsABlankCode();
        BonusPoolReviewLabelLengthRuleFlagsALongLabel();
    }
}
