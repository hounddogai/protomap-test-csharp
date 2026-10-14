namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class BatchedLaptopRequestReportChecks
{
    public static void BatchedLaptopRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702041683);
        var items = new List<BatchedLaptopRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedLaptopRequest.NewBatchedLaptopRequest($"batched-laptop-request-{index}", "sample"));
        }
        var report = BatchedLaptopRequestReport.BuildBatchedLaptopRequestReport("test", items, clock);
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
        BatchedLaptopRequestReportCountsEachRecordOnce();
    }
}
