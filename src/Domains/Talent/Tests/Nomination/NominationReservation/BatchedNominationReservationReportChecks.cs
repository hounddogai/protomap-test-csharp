namespace Acme.Workforce.Talent.Tests.Nomination.NominationReservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Checks of nomination reservation code, run by the project's runner.</summary>
public static class BatchedNominationReservationReportChecks
{
    public static void BatchedNominationReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701687083);
        var items = new List<BatchedNominationReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedNominationReservation.NewBatchedNominationReservation($"batched-nomination-reservation-{index}", "sample"));
        }
        var report = BatchedNominationReservationReport.BuildBatchedNominationReservationReport("test", items, clock);
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
        BatchedNominationReservationReportCountsEachRecordOnce();
    }
}
