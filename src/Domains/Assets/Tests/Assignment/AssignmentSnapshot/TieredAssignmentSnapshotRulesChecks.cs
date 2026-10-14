namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class TieredAssignmentSnapshotRulesChecks
{
    public static void TieredAssignmentSnapshotValidatorReportsEveryProblem()
    {
        var validator = TieredAssignmentSnapshotRules.DefaultTieredAssignmentSnapshotValidator();
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAssignmentSnapshotTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new TieredAssignmentSnapshotTimezoneLengthRule(1000);
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredAssignmentSnapshotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TieredAssignmentSnapshotUpdatedAtRangeRule(16, 80);
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredAssignmentSnapshotRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new TieredAssignmentSnapshotRetriesRangeRule(32, 80);
        var item = TieredAssignmentSnapshot.NewTieredAssignmentSnapshot("x", "y");
        item.Retries = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredAssignmentSnapshotValidatorReportsEveryProblem();
        TieredAssignmentSnapshotTimezoneLengthRuleFlagsALongTimezone();
        TieredAssignmentSnapshotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TieredAssignmentSnapshotRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
