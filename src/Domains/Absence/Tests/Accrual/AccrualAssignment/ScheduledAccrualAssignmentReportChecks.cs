namespace Acme.Workforce.Absence.Tests.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.Accrual.AccrualAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of accrual assignment code, run by the project's runner.</summary>
public static class ScheduledAccrualAssignmentReportChecks
{
    public static void ScheduledAccrualAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704349474);
        var items = new List<ScheduledAccrualAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledAccrualAssignment.NewScheduledAccrualAssignment($"scheduled-accrual-assignment-{index}", "sample"));
        }
        var report = ScheduledAccrualAssignmentReport.BuildScheduledAccrualAssignmentReport("test", items, clock);
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
        ScheduledAccrualAssignmentReportCountsEachRecordOnce();
    }
}
