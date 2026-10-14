namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupReportChecks
{
    public static void PrimaryMeritCycleRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701484357);
        var items = new List<PrimaryMeritCycleRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup($"primary-merit-cycle-rollup-{index}", "sample"));
        }
        var report = PrimaryMeritCycleRollupReport.BuildPrimaryMeritCycleRollupReport("test", items, clock);
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
        PrimaryMeritCycleRollupReportCountsEachRecordOnce();
    }
}
