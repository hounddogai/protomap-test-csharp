namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class BatchedMobilityAllocationReportChecks
{
    public static void BatchedMobilityAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700309467);
        var items = new List<BatchedMobilityAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedMobilityAllocation.NewBatchedMobilityAllocation($"batched-mobility-allocation-{index}", "sample"));
        }
        var report = BatchedMobilityAllocationReport.BuildBatchedMobilityAllocationReport("test", items, clock);
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
        BatchedMobilityAllocationReportCountsEachRecordOnce();
    }
}
