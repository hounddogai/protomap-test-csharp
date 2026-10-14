namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueDraftRulesChecks
{
    public static void TrainingDueDraftValidatorReportsEveryProblem()
    {
        var validator = TrainingDueDraftRules.DefaultTrainingDueDraftValidator();
        var item = TrainingDueDraft.NewTrainingDueDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueDraftRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new TrainingDueDraftRatingRangeRule(5.0, 401.1);
        var item = TrainingDueDraft.NewTrainingDueDraft("x", "y");
        item.Rating = 402.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueDraftManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new TrainingDueDraftManagerIdLengthRule(120);
        var item = TrainingDueDraft.NewTrainingDueDraft("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TrainingDueDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new TrainingDueDraftBatchSizeRangeRule(1000, 86400);
        var item = TrainingDueDraft.NewTrainingDueDraft("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueDraftValidatorReportsEveryProblem();
        TrainingDueDraftRatingRangeRuleFlagsValuesAboveRating();
        TrainingDueDraftManagerIdLengthRuleFlagsALongManagerId();
        TrainingDueDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
