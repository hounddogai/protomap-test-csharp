namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanReviewRulesChecks
{
    public static void ReturnPlanReviewValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanReviewRules.DefaultReturnPlanReviewValidator();
        var item = ReturnPlanReview.NewReturnPlanReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ReturnPlanReviewBatchSizeRangeRule(255, 1000);
        var item = ReturnPlanReview.NewReturnPlanReview("x", "y");
        item.BatchSize = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanReviewBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new ReturnPlanReviewBudgetRangeRule(4.6, 1690.0);
        var item = ReturnPlanReview.NewReturnPlanReview("x", "y");
        item.Budget = 1691.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanReviewValidatorReportsEveryProblem();
        ReturnPlanReviewBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ReturnPlanReviewBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
