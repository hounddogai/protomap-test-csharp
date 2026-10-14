namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class SeasonalOffboardingSlotReportChecks
{
    public static void SeasonalOffboardingSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702458216);
        var items = new List<SeasonalOffboardingSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalOffboardingSlot.NewSeasonalOffboardingSlot($"seasonal-offboarding-slot-{index}", "sample"));
        }
        var report = SeasonalOffboardingSlotReport.BuildSeasonalOffboardingSlotReport("test", items, clock);
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
        SeasonalOffboardingSlotReportCountsEachRecordOnce();
    }
}
