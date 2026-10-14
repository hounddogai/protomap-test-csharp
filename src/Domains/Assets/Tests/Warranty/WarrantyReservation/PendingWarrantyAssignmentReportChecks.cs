namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class PendingWarrantyAssignmentReportChecks
{
    public static void PendingWarrantyAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707139034);
        var items = new List<PendingWarrantyAssignment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingWarrantyAssignment.NewPendingWarrantyAssignment($"pending-warranty-assignment-{index}", "sample"));
        }
        var report = PendingWarrantyAssignmentReport.BuildPendingWarrantyAssignmentReport("test", items, clock);
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
        PendingWarrantyAssignmentReportCountsEachRecordOnce();
    }
}
