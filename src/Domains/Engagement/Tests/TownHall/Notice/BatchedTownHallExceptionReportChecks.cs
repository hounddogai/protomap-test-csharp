namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedTownHallExceptionReportChecks
{
    public static void BatchedTownHallExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701079547);
        var items = new List<BatchedTownHallException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BatchedTownHallException.NewBatchedTownHallException($"batched-town-hall-exception-{index}", "sample"));
        }
        var report = BatchedTownHallExceptionReport.BuildBatchedTownHallExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BatchedTownHallExceptionReportCountsEachRecordOnce();
    }
}
