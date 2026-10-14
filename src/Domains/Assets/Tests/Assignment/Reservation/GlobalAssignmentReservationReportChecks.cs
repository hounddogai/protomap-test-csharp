namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GlobalAssignmentReservationReportChecks
{
    public static void GlobalAssignmentReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700838012);
        var items = new List<GlobalAssignmentReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(GlobalAssignmentReservation.NewGlobalAssignmentReservation($"global-assignment-reservation-{index}", "sample"));
        }
        var report = GlobalAssignmentReservationReport.BuildGlobalAssignmentReservationReport("test", items, clock);
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
        GlobalAssignmentReservationReportCountsEachRecordOnce();
    }
}
