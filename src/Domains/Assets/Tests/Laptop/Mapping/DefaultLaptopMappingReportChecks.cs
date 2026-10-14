namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopMappingReportChecks
{
    public static void DefaultLaptopMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705718556);
        var items = new List<DefaultLaptopMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(DefaultLaptopMapping.NewDefaultLaptopMapping($"default-laptop-mapping-{index}", "sample"));
        }
        var report = DefaultLaptopMappingReport.BuildDefaultLaptopMappingReport("test", items, clock);
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
        DefaultLaptopMappingReportCountsEachRecordOnce();
    }
}
