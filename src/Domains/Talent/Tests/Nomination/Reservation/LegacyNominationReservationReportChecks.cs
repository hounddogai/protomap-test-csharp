namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyNominationReservationReportChecks
{
    public static void LegacyNominationReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700244289);
        var items = new List<LegacyNominationReservation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyNominationReservation.NewLegacyNominationReservation($"legacy-nomination-reservation-{index}", "sample"));
        }
        var report = LegacyNominationReservationReport.BuildLegacyNominationReservationReport("test", items, clock);
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
        LegacyNominationReservationReportCountsEachRecordOnce();
    }
}
