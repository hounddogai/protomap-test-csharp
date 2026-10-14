namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountScoreReportChecks
{
    public static void HeadcountScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703901295);
        var items = new List<HeadcountScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HeadcountScore.NewHeadcountScore($"headcount-score-{index}", "sample"));
        }
        var report = HeadcountScoreReport.BuildHeadcountScoreReport("test", items, clock);
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
        HeadcountScoreReportCountsEachRecordOnce();
    }
}
