namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class LenientLaptopExceptionReportChecks
{
    public static void LenientLaptopExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702509101);
        var items = new List<LenientLaptopException>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientLaptopException.NewLenientLaptopException($"lenient-laptop-exception-{index}", "sample"));
        }
        var report = LenientLaptopExceptionReport.BuildLenientLaptopExceptionReport("test", items, clock);
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
        LenientLaptopExceptionReportCountsEachRecordOnce();
    }
}
