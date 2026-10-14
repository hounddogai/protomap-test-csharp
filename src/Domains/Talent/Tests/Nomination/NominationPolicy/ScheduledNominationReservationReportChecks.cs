namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationReservationReportChecks
{
    public static void ScheduledNominationReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702439573);
        var items = new List<ScheduledNominationReservation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledNominationReservation.NewScheduledNominationReservation($"scheduled-nomination-reservation-{index}", "sample"));
        }
        var report = ScheduledNominationReservationReport.BuildScheduledNominationReservationReport("test", items, clock);
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
        ScheduledNominationReservationReportCountsEachRecordOnce();
    }
}
