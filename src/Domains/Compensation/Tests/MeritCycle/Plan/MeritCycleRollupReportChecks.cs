namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleRollupReportChecks
{
    public static void MeritCycleRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702914439);
        var items = new List<MeritCycleRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(MeritCycleRollup.NewMeritCycleRollup($"merit-cycle-rollup-{index}", "sample"));
        }
        var report = MeritCycleRollupReport.BuildMeritCycleRollupReport("test", items, clock);
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
        MeritCycleRollupReportCountsEachRecordOnce();
    }
}
