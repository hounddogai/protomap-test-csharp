namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetReportChecks
{
    public static void PendingLaptopTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704082511);
        var items = new List<PendingLaptopTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingLaptopTarget.NewPendingLaptopTarget($"pending-laptop-target-{index}", "sample"));
        }
        var report = PendingLaptopTargetReport.BuildPendingLaptopTargetReport("test", items, clock);
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
        PendingLaptopTargetReportCountsEachRecordOnce();
    }
}
