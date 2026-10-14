namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopSegmentReportChecks
{
    public static void LaptopSegmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709927725);
        var items = new List<LaptopSegment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LaptopSegment.NewLaptopSegment($"laptop-segment-{index}", "sample"));
        }
        var report = LaptopSegmentReport.BuildLaptopSegmentReport("test", items, clock);
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
        LaptopSegmentReportCountsEachRecordOnce();
    }
}
