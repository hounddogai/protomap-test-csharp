namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class ScheduledAccrualBaselineReportChecks
{
    public static void ScheduledAccrualBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704805378);
        var items = new List<ScheduledAccrualBaseline>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ScheduledAccrualBaseline.NewScheduledAccrualBaseline($"scheduled-accrual-baseline-{index}", "sample"));
        }
        var report = ScheduledAccrualBaselineReport.BuildScheduledAccrualBaselineReport("test", items, clock);
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
        ScheduledAccrualBaselineReportCountsEachRecordOnce();
    }
}
