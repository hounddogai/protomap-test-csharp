namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleThresholdReportChecks
{
    public static void MeritCycleThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705499987);
        var items = new List<MeritCycleThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(MeritCycleThreshold.NewMeritCycleThreshold($"merit-cycle-threshold-{index}", "sample"));
        }
        var report = MeritCycleThresholdReport.BuildMeritCycleThresholdReport("test", items, clock);
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
        MeritCycleThresholdReportCountsEachRecordOnce();
    }
}
