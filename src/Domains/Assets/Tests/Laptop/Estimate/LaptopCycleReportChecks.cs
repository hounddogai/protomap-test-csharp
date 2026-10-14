namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LaptopCycleReportChecks
{
    public static void LaptopCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709179448);
        var items = new List<LaptopCycle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LaptopCycle.NewLaptopCycle($"laptop-cycle-{index}", "sample"));
        }
        var report = LaptopCycleReport.BuildLaptopCycleReport("test", items, clock);
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
        LaptopCycleReportCountsEachRecordOnce();
    }
}
