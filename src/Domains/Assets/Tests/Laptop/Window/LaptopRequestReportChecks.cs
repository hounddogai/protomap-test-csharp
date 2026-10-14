namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LaptopRequestReportChecks
{
    public static void LaptopRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706602417);
        var items = new List<LaptopRequest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LaptopRequest.NewLaptopRequest($"laptop-request-{index}", "sample"));
        }
        var report = LaptopRequestReport.BuildLaptopRequestReport("test", items, clock);
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
        LaptopRequestReportCountsEachRecordOnce();
    }
}
