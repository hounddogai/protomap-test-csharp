namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentBatchReportChecks
{
    public static void AdjustmentBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702180986);
        var items = new List<AdjustmentBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdjustmentBatch.NewAdjustmentBatch($"adjustment-batch-{index}", "sample"));
        }
        var report = AdjustmentBatchReport.BuildAdjustmentBatchReport("test", items, clock);
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
        AdjustmentBatchReportCountsEachRecordOnce();
    }
}
