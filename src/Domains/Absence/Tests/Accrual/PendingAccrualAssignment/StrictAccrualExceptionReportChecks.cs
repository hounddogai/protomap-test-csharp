namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class StrictAccrualExceptionReportChecks
{
    public static void StrictAccrualExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706131175);
        var items = new List<StrictAccrualException>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(StrictAccrualException.NewStrictAccrualException($"strict-accrual-exception-{index}", "sample"));
        }
        var report = StrictAccrualExceptionReport.BuildStrictAccrualExceptionReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        StrictAccrualExceptionReportCountsEachRecordOnce();
    }
}
