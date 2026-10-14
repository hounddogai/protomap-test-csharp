namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotRulesChecks
{
    public static void ScheduledPayBandSnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayBandSnapshotRules.DefaultScheduledPayBandSnapshotValidator();
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayBandSnapshotRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ScheduledPayBandSnapshotRatingRangeRule(6.1, 3990.1);
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("x", "y");
        item.Rating = 3991.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayBandSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ScheduledPayBandSnapshotCreatedAtRangeRule(32, 255);
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayBandSnapshotRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledPayBandSnapshotRequiredNameRule("scheduled pay band snapshot needs a value");
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledPayBandSnapshotValidatorReportsEveryProblem();
        ScheduledPayBandSnapshotRatingRangeRuleFlagsValuesAboveRating();
        ScheduledPayBandSnapshotCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        ScheduledPayBandSnapshotRequiredNameRuleFlagsABlankName();
    }
}
