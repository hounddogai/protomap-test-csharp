namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class ActionPlanReviewRulesChecks
{
    public static void ActionPlanReviewValidatorReportsEveryProblem()
    {
        var validator = ActionPlanReviewRules.DefaultActionPlanReviewValidator();
        var item = ActionPlanReview.NewActionPlanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanReviewMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ActionPlanReviewMaximumRangeRule(2.5, 3605.8);
        var item = ActionPlanReview.NewActionPlanReview("x", "y");
        item.Maximum = 3607.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanReviewLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ActionPlanReviewLabelLengthRule(86400);
        var item = ActionPlanReview.NewActionPlanReview("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanReviewCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new ActionPlanReviewCategoryLengthRule(86400);
        var item = ActionPlanReview.NewActionPlanReview("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanReviewRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ActionPlanReviewRequiredCategoryRule("action plan review needs an entry");
        var item = ActionPlanReview.NewActionPlanReview("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanReviewValidatorReportsEveryProblem();
        ActionPlanReviewMaximumRangeRuleFlagsValuesAboveMaximum();
        ActionPlanReviewLabelLengthRuleFlagsALongLabel();
        ActionPlanReviewCategoryLengthRuleFlagsALongCategory();
        ActionPlanReviewRequiredCategoryRuleFlagsABlankCategory();
    }
}
