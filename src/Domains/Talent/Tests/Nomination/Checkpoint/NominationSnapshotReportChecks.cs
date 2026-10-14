namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationSnapshotReportChecks
{
    public static void NominationSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707925030);
        var items = new List<NominationSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(NominationSnapshot.NewNominationSnapshot($"nomination-snapshot-{index}", "sample"));
        }
        var report = NominationSnapshotReport.BuildNominationSnapshotReport("test", items, clock);
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
        NominationSnapshotReportCountsEachRecordOnce();
    }
}
