namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleReservationReportChecks
{
    public static void AdaptiveMeritCycleReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702681435);
        var items = new List<AdaptiveMeritCycleReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation($"adaptive-merit-cycle-reservation-{index}", "sample"));
        }
        var report = AdaptiveMeritCycleReservationReport.BuildAdaptiveMeritCycleReservationReport("test", items, clock);
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
        AdaptiveMeritCycleReservationReportCountsEachRecordOnce();
    }
}
