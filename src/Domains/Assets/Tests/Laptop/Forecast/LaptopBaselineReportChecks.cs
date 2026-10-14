namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopBaselineReportChecks
{
    public static void LaptopBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702268109);
        var items = new List<LaptopBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LaptopBaseline.NewLaptopBaseline($"laptop-baseline-{index}", "sample"));
        }
        var report = LaptopBaselineReport.BuildLaptopBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        LaptopBaselineReportCountsEachRecordOnce();
    }
}
