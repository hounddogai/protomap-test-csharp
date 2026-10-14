namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BonusPoolEstimateReportChecks
{
    public static void BonusPoolEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701421438);
        var items = new List<BonusPoolEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(BonusPoolEstimate.NewBonusPoolEstimate($"bonus-pool-estimate-{index}", "sample"));
        }
        var report = BonusPoolEstimateReport.BuildBonusPoolEstimateReport("test", items, clock);
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
        BonusPoolEstimateReportCountsEachRecordOnce();
    }
}
