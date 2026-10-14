namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEntryReportChecks
{
    public static void SeasonalPolicyAckEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708082664);
        var items = new List<SeasonalPolicyAckEntry>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry($"seasonal-policy-ack-entry-{index}", "sample"));
        }
        var report = SeasonalPolicyAckEntryReport.BuildSeasonalPolicyAckEntryReport("test", items, clock);
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
        SeasonalPolicyAckEntryReportCountsEachRecordOnce();
    }
}
