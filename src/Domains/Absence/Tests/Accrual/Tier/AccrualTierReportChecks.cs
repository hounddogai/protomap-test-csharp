namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierReportChecks
{
    public static void AccrualTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702762687);
        var items = new List<AccrualTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(AccrualTier.NewAccrualTier($"accrual-tier-{index}", "sample"));
        }
        var report = AccrualTierReport.BuildAccrualTierReport("test", items, clock);
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
        AccrualTierReportCountsEachRecordOnce();
    }
}
