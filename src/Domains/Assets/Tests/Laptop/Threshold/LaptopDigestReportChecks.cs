namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopDigestReportChecks
{
    public static void LaptopDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703919478);
        var items = new List<LaptopDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LaptopDigest.NewLaptopDigest($"laptop-digest-{index}", "sample"));
        }
        var report = LaptopDigestReport.BuildLaptopDigestReport("test", items, clock);
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
        LaptopDigestReportCountsEachRecordOnce();
    }
}
