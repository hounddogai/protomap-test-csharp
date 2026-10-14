namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class RollingAccrualAssignmentReportChecks
{
    public static void RollingAccrualAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702609228);
        var items = new List<RollingAccrualAssignment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingAccrualAssignment.NewRollingAccrualAssignment($"rolling-accrual-assignment-{index}", "sample"));
        }
        var report = RollingAccrualAssignmentReport.BuildRollingAccrualAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingAccrualAssignmentReportCountsEachRecordOnce();
    }
}
