namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class TieredMobilityReservationReportChecks
{
    public static void TieredMobilityReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709486249);
        var items = new List<TieredMobilityReservation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredMobilityReservation.NewTieredMobilityReservation($"tiered-mobility-reservation-{index}", "sample"));
        }
        var report = TieredMobilityReservationReport.BuildTieredMobilityReservationReport("test", items, clock);
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
        TieredMobilityReservationReportCountsEachRecordOnce();
    }
}
