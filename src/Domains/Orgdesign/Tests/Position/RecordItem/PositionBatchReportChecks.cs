namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionBatchReportChecks
{
    public static void PositionBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704921448);
        var items = new List<PositionBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PositionBatch.NewPositionBatch($"position-batch-{index}", "sample"));
        }
        var report = PositionBatchReport.BuildPositionBatchReport("test", items, clock);
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
        PositionBatchReportCountsEachRecordOnce();
    }
}
