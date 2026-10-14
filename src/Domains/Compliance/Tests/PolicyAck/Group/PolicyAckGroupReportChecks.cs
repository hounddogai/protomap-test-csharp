namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckGroupReportChecks
{
    public static void PolicyAckGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707426186);
        var items = new List<PolicyAckGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PolicyAckGroup.NewPolicyAckGroup($"policy-ack-group-{index}", "sample"));
        }
        var report = PolicyAckGroupReport.BuildPolicyAckGroupReport("test", items, clock);
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
        PolicyAckGroupReportCountsEachRecordOnce();
    }
}
