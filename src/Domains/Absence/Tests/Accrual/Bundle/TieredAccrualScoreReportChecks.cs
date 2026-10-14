namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualScoreReportChecks
{
    public static void TieredAccrualScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701110594);
        var items = new List<TieredAccrualScore>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredAccrualScore.NewTieredAccrualScore($"tiered-accrual-score-{index}", "sample"));
        }
        var report = TieredAccrualScoreReport.BuildTieredAccrualScoreReport("test", items, clock);
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
        TieredAccrualScoreReportCountsEachRecordOnce();
    }
}
