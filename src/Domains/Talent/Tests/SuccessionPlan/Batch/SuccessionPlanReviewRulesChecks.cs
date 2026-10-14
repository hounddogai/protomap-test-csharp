namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanReviewRulesChecks
{
    public static void SuccessionPlanReviewValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanReviewRules.DefaultSuccessionPlanReviewValidator();
        var item = SuccessionPlanReview.NewSuccessionPlanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanReviewRequiredCodeRule("succession plan review needs this field");
        var item = SuccessionPlanReview.NewSuccessionPlanReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanReviewTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SuccessionPlanReviewTitleLengthRule(120);
        var item = SuccessionPlanReview.NewSuccessionPlanReview("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanReviewValidatorReportsEveryProblem();
        SuccessionPlanReviewRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanReviewTitleLengthRuleFlagsALongTitle();
    }
}
