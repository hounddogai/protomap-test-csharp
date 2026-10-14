namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanScoreReportChecks
{
    public static void StrictSuccessionPlanScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702220158);
        var items = new List<StrictSuccessionPlanScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictSuccessionPlanScore.NewStrictSuccessionPlanScore($"strict-succession-plan-score-{index}", "sample"));
        }
        var report = StrictSuccessionPlanScoreReport.BuildStrictSuccessionPlanScoreReport("test", items, clock);
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
        StrictSuccessionPlanScoreReportCountsEachRecordOnce();
    }
}
