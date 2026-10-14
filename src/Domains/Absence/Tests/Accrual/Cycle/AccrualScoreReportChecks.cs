namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualScoreReportChecks
{
    public static void AccrualScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702918975);
        var items = new List<AccrualScore>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccrualScore.NewAccrualScore($"accrual-score-{index}", "sample"));
        }
        var report = AccrualScoreReport.BuildAccrualScoreReport("test", items, clock);
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
        AccrualScoreReportCountsEachRecordOnce();
    }
}
