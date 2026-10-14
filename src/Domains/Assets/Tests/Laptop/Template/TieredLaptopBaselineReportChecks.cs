namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredLaptopBaselineReportChecks
{
    public static void TieredLaptopBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702428465);
        var items = new List<TieredLaptopBaseline>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredLaptopBaseline.NewTieredLaptopBaseline($"tiered-laptop-baseline-{index}", "sample"));
        }
        var report = TieredLaptopBaselineReport.BuildTieredLaptopBaselineReport("test", items, clock);
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
        TieredLaptopBaselineReportCountsEachRecordOnce();
    }
}
