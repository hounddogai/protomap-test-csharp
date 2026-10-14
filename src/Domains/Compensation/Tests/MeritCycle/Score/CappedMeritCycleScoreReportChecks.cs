namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CappedMeritCycleScoreReportChecks
{
    public static void CappedMeritCycleScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706169646);
        var items = new List<CappedMeritCycleScore>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedMeritCycleScore.NewCappedMeritCycleScore($"capped-merit-cycle-score-{index}", "sample"));
        }
        var report = CappedMeritCycleScoreReport.BuildCappedMeritCycleScoreReport("test", items, clock);
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
        CappedMeritCycleScoreReportCountsEachRecordOnce();
    }
}
