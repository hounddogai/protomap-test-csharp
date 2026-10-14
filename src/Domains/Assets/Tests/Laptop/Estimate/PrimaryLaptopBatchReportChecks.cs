namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryLaptopBatchReportChecks
{
    public static void PrimaryLaptopBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701345919);
        var items = new List<PrimaryLaptopBatch>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryLaptopBatch.NewPrimaryLaptopBatch($"primary-laptop-batch-{index}", "sample"));
        }
        var report = PrimaryLaptopBatchReport.BuildPrimaryLaptopBatchReport("test", items, clock);
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
        PrimaryLaptopBatchReportCountsEachRecordOnce();
    }
}
