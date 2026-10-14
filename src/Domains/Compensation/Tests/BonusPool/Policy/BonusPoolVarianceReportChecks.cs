namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolVarianceReportChecks
{
    public static void BonusPoolVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706490258);
        var items = new List<BonusPoolVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BonusPoolVariance.NewBonusPoolVariance($"bonus-pool-variance-{index}", "sample"));
        }
        var report = BonusPoolVarianceReport.BuildBonusPoolVarianceReport("test", items, clock);
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
        BonusPoolVarianceReportCountsEachRecordOnce();
    }
}
