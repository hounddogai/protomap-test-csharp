namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultBonusPoolCheckpointReportChecks
{
    public static void DefaultBonusPoolCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705053621);
        var items = new List<DefaultBonusPoolCheckpoint>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultBonusPoolCheckpoint.NewDefaultBonusPoolCheckpoint($"default-bonus-pool-checkpoint-{index}", "sample"));
        }
        var report = DefaultBonusPoolCheckpointReport.BuildDefaultBonusPoolCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultBonusPoolCheckpointReportCountsEachRecordOnce();
    }
}
