namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathSlotReportChecks
{
    public static void CareerPathSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701101679);
        var items = new List<CareerPathSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CareerPathSlot.NewCareerPathSlot($"career-path-slot-{index}", "sample"));
        }
        var report = CareerPathSlotReport.BuildCareerPathSlotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        CareerPathSlotReportCountsEachRecordOnce();
    }
}
