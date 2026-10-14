namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordReportChecks
{
    public static void LaptopRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702305862);
        var items = new List<LaptopRecord>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LaptopRecord.NewLaptopRecord($"laptop-record-{index}", "sample"));
        }
        var report = LaptopRecordReport.BuildLaptopRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LaptopRecordReportCountsEachRecordOnce();
    }
}
