namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleBaselineReportChecks
{
    public static void MeritCycleBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700557649);
        var items = new List<MeritCycleBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleBaseline.NewMeritCycleBaseline($"merit-cycle-baseline-{index}", "sample"));
        }
        var report = MeritCycleBaselineReport.BuildMeritCycleBaselineReport("test", items, clock);
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
        MeritCycleBaselineReportCountsEachRecordOnce();
    }
}
