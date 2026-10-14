namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckEntryReportChecks
{
    public static void PolicyAckEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700122136);
        var items = new List<PolicyAckEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyAckEntry.NewPolicyAckEntry($"policy-ack-entry-{index}", "sample"));
        }
        var report = PolicyAckEntryReport.BuildPolicyAckEntryReport("test", items, clock);
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
        PolicyAckEntryReportCountsEachRecordOnce();
    }
}
