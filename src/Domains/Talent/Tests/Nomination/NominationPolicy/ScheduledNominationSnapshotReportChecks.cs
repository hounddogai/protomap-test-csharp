namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationSnapshotReportChecks
{
    public static void ScheduledNominationSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706473861);
        var items = new List<ScheduledNominationSnapshot>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ScheduledNominationSnapshot.NewScheduledNominationSnapshot($"scheduled-nomination-snapshot-{index}", "sample"));
        }
        var report = ScheduledNominationSnapshotReport.BuildScheduledNominationSnapshotReport("test", items, clock);
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
        ScheduledNominationSnapshotReportCountsEachRecordOnce();
    }
}
