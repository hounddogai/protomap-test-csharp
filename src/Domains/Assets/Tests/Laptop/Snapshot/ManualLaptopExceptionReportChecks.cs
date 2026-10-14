namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualLaptopExceptionReportChecks
{
    public static void ManualLaptopExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706219382);
        var items = new List<ManualLaptopException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualLaptopException.NewManualLaptopException($"manual-laptop-exception-{index}", "sample"));
        }
        var report = ManualLaptopExceptionReport.BuildManualLaptopExceptionReport("test", items, clock);
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
        ManualLaptopExceptionReportCountsEachRecordOnce();
    }
}
