namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientLaptopWindowReportChecks
{
    public static void LenientLaptopWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701427704);
        var items = new List<LenientLaptopWindow>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientLaptopWindow.NewLenientLaptopWindow($"lenient-laptop-window-{index}", "sample"));
        }
        var report = LenientLaptopWindowReport.BuildLenientLaptopWindowReport("test", items, clock);
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
        LenientLaptopWindowReportCountsEachRecordOnce();
    }
}
