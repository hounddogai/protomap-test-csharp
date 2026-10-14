namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointReportChecks
{
    public static void WeightedBonusPoolCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709960098);
        var items = new List<WeightedBonusPoolCheckpoint>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint($"weighted-bonus-pool-checkpoint-{index}", "sample"));
        }
        var report = WeightedBonusPoolCheckpointReport.BuildWeightedBonusPoolCheckpointReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedBonusPoolCheckpointReportCountsEachRecordOnce();
    }
}
