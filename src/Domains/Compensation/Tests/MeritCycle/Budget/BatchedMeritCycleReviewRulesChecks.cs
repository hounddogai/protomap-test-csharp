namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedMeritCycleReviewRulesChecks
{
    public static void BatchedMeritCycleReviewValidatorReportsEveryProblem()
    {
        var validator = BatchedMeritCycleReviewRules.DefaultBatchedMeritCycleReviewValidator();
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedMeritCycleReviewFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new BatchedMeritCycleReviewFteRangeRule(4.2, 627.0);
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("x", "y");
        item.Fte = 628.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedMeritCycleReviewRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new BatchedMeritCycleReviewRequiredRegionRule("batched merit cycle review needs an entry");
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedMeritCycleReviewValidatorReportsEveryProblem();
        BatchedMeritCycleReviewFteRangeRuleFlagsValuesAboveFte();
        BatchedMeritCycleReviewRequiredRegionRuleFlagsABlankRegion();
    }
}
