namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryAccrualTierReportChecks
{
    public static void PrimaryAccrualTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701721803);
        var items = new List<PrimaryAccrualTier>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryAccrualTier.NewPrimaryAccrualTier($"primary-accrual-tier-{index}", "sample"));
        }
        var report = PrimaryAccrualTierReport.BuildPrimaryAccrualTierReport("test", items, clock);
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
        PrimaryAccrualTierReportCountsEachRecordOnce();
    }
}
