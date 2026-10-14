namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopWindowReportChecks
{
    public static void LaptopWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706072757);
        var items = new List<LaptopWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LaptopWindow.NewLaptopWindow($"laptop-window-{index}", "sample"));
        }
        var report = LaptopWindowReport.BuildLaptopWindowReport("test", items, clock);
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
        LaptopWindowReportCountsEachRecordOnce();
    }
}
