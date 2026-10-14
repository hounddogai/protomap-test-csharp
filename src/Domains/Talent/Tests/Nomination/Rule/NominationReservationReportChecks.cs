namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationReservationReportChecks
{
    public static void NominationReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705285029);
        var items = new List<NominationReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(NominationReservation.NewNominationReservation($"nomination-reservation-{index}", "sample"));
        }
        var report = NominationReservationReport.BuildNominationReservationReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        NominationReservationReportCountsEachRecordOnce();
    }
}
