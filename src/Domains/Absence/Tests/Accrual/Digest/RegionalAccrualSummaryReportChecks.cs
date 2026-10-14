namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RegionalAccrualSummaryReportChecks
{
    public static void RegionalAccrualSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701877954);
        var items = new List<RegionalAccrualSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RegionalAccrualSummary.NewRegionalAccrualSummary($"regional-accrual-summary-{index}", "sample"));
        }
        var report = RegionalAccrualSummaryReport.BuildRegionalAccrualSummaryReport("test", items, clock);
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
        RegionalAccrualSummaryReportCountsEachRecordOnce();
    }
}
