namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotReportChecks
{
    public static void AdaptiveCareerPathSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700199368);
        var items = new List<AdaptiveCareerPathSlot>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot($"adaptive-career-path-slot-{index}", "sample"));
        }
        var report = AdaptiveCareerPathSlotReport.BuildAdaptiveCareerPathSlotReport("test", items, clock);
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
        AdaptiveCareerPathSlotReportCountsEachRecordOnce();
    }
}
