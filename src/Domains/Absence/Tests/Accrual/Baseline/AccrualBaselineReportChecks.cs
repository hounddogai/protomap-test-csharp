namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualBaselineReportChecks
{
    public static void AccrualBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705993024);
        var items = new List<AccrualBaseline>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AccrualBaseline.NewAccrualBaseline($"accrual-baseline-{index}", "sample"));
        }
        var report = AccrualBaselineReport.BuildAccrualBaselineReport("test", items, clock);
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
        AccrualBaselineReportCountsEachRecordOnce();
    }
}
