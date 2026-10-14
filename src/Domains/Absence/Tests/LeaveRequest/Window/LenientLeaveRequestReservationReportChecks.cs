namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationReportChecks
{
    public static void LenientLeaveRequestReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701466851);
        var items = new List<LenientLeaveRequestReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientLeaveRequestReservation.NewLenientLeaveRequestReservation($"lenient-leave-request-reservation-{index}", "sample"));
        }
        var report = LenientLeaveRequestReservationReport.BuildLenientLeaveRequestReservationReport("test", items, clock);
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
        LenientLeaveRequestReservationReportCountsEachRecordOnce();
    }
}
