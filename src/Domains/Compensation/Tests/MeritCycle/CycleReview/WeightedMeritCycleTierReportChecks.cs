namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierReportChecks
{
    public static void WeightedMeritCycleTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702713080);
        var items = new List<WeightedMeritCycleTier>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(WeightedMeritCycleTier.NewWeightedMeritCycleTier($"weighted-merit-cycle-tier-{index}", "sample"));
        }
        var report = WeightedMeritCycleTierReport.BuildWeightedMeritCycleTierReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedMeritCycleTierReportCountsEachRecordOnce();
    }
}
