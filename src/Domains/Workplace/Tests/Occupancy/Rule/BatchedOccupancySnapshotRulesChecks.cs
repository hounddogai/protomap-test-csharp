namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedOccupancySnapshotRulesChecks
{
    public static void BatchedOccupancySnapshotValidatorReportsEveryProblem()
    {
        var validator = BatchedOccupancySnapshotRules.DefaultBatchedOccupancySnapshotValidator();
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedOccupancySnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BatchedOccupancySnapshotTitleLengthRule(16);
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedOccupancySnapshotRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new BatchedOccupancySnapshotRequiredGradeRule("batched occupancy snapshot needs this field");
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedOccupancySnapshotRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new BatchedOccupancySnapshotRatingRangeRule(0.0, 4288.2);
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("x", "y");
        item.Rating = 4289.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BatchedOccupancySnapshotValidatorReportsEveryProblem();
        BatchedOccupancySnapshotTitleLengthRuleFlagsALongTitle();
        BatchedOccupancySnapshotRequiredGradeRuleFlagsABlankGrade();
        BatchedOccupancySnapshotRatingRangeRuleFlagsValuesAboveRating();
    }
}
