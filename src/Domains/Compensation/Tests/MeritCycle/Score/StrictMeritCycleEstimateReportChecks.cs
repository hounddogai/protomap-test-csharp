namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateReportChecks
{
    public static void StrictMeritCycleEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705479780);
        var items = new List<StrictMeritCycleEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictMeritCycleEstimate.NewStrictMeritCycleEstimate($"strict-merit-cycle-estimate-{index}", "sample"));
        }
        var report = StrictMeritCycleEstimateReport.BuildStrictMeritCycleEstimateReport("test", items, clock);
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
        StrictMeritCycleEstimateReportCountsEachRecordOnce();
    }
}
