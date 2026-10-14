namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of grant metric code, run by the project's runner.</summary>
public static class EquityGrantScoreReportChecks
{
    public static void EquityGrantScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708969938);
        var items = new List<EquityGrantScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EquityGrantScore.NewEquityGrantScore($"equity-grant-score-{index}", "sample"));
        }
        var report = EquityGrantScoreReport.BuildEquityGrantScoreReport("test", items, clock);
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
        EquityGrantScoreReportCountsEachRecordOnce();
    }
}
