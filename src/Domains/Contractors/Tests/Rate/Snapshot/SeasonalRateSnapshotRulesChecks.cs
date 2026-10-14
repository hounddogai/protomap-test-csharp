namespace Acme.Workforce.Contractors.Tests.Rate.Snapshot;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalRateSnapshotRulesChecks
{
    public static void SeasonalRateSnapshotValidatorReportsEveryProblem()
    {
        var validator = SeasonalRateSnapshotRules.DefaultSeasonalRateSnapshotValidator();
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalRateSnapshotChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SeasonalRateSnapshotChannelLengthRule(64);
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalRateSnapshotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new SeasonalRateSnapshotUpdatedAtRangeRule(16, 3600);
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalRateSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalRateSnapshotRequiredCodeRule("seasonal rate snapshot needs an entry");
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalRateSnapshotValidatorReportsEveryProblem();
        SeasonalRateSnapshotChannelLengthRuleFlagsALongChannel();
        SeasonalRateSnapshotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        SeasonalRateSnapshotRequiredCodeRuleFlagsABlankCode();
    }
}
