namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedOccupancyBatchReportChecks
{
    public static void WeightedOccupancyBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700003765);
        var items = new List<WeightedOccupancyBatch>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(WeightedOccupancyBatch.NewWeightedOccupancyBatch($"weighted-occupancy-batch-{index}", "sample"));
        }
        var report = WeightedOccupancyBatchReport.BuildWeightedOccupancyBatchReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedOccupancyBatchReportCountsEachRecordOnce();
    }
}
