namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountBatchReportChecks
{
    public static void HeadcountBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702945636);
        var items = new List<HeadcountBatch>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HeadcountBatch.NewHeadcountBatch($"headcount-batch-{index}", "sample"));
        }
        var report = HeadcountBatchReport.BuildHeadcountBatchReport("test", items, clock);
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
        HeadcountBatchReportCountsEachRecordOnce();
    }
}
