namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupReportChecks
{
    public static void SeasonalMeritCycleRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708525653);
        var items = new List<SeasonalMeritCycleRollup>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup($"seasonal-merit-cycle-rollup-{index}", "sample"));
        }
        var report = SeasonalMeritCycleRollupReport.BuildSeasonalMeritCycleRollupReport("test", items, clock);
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
        SeasonalMeritCycleRollupReportCountsEachRecordOnce();
    }
}
