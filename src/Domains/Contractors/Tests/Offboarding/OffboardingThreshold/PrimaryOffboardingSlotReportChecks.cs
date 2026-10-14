namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class PrimaryOffboardingSlotReportChecks
{
    public static void PrimaryOffboardingSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700120083);
        var items = new List<PrimaryOffboardingSlot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryOffboardingSlot.NewPrimaryOffboardingSlot($"primary-offboarding-slot-{index}", "sample"));
        }
        var report = PrimaryOffboardingSlotReport.BuildPrimaryOffboardingSlotReport("test", items, clock);
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
        PrimaryOffboardingSlotReportCountsEachRecordOnce();
    }
}
