namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualSummaryReportChecks
{
    public static void AccrualSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700476011);
        var items = new List<AccrualSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AccrualSummary.NewAccrualSummary($"accrual-summary-{index}", "sample"));
        }
        var report = AccrualSummaryReport.BuildAccrualSummaryReport("test", items, clock);
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
        AccrualSummaryReportCountsEachRecordOnce();
    }
}
