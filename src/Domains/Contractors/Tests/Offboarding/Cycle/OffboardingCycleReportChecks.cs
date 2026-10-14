namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleReportChecks
{
    public static void OffboardingCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709268878);
        var items = new List<OffboardingCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OffboardingCycle.NewOffboardingCycle($"offboarding-cycle-{index}", "sample"));
        }
        var report = OffboardingCycleReport.BuildOffboardingCycleReport("test", items, clock);
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
        OffboardingCycleReportCountsEachRecordOnce();
    }
}
