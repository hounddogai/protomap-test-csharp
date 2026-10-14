namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientAccrualAssignmentReportChecks
{
    public static void LenientAccrualAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706346264);
        var items = new List<LenientAccrualAssignment>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LenientAccrualAssignment.NewLenientAccrualAssignment($"lenient-accrual-assignment-{index}", "sample"));
        }
        var report = LenientAccrualAssignmentReport.BuildLenientAccrualAssignmentReport("test", items, clock);
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
        LenientAccrualAssignmentReportCountsEachRecordOnce();
    }
}
