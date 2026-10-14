namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PendingOffboardingCycleReportChecks
{
    public static void PendingOffboardingCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705842892);
        var items = new List<PendingOffboardingCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingOffboardingCycle.NewPendingOffboardingCycle($"pending-offboarding-cycle-{index}", "sample"));
        }
        var report = PendingOffboardingCycleReport.BuildPendingOffboardingCycleReport("test", items, clock);
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
        PendingOffboardingCycleReportCountsEachRecordOnce();
    }
}
