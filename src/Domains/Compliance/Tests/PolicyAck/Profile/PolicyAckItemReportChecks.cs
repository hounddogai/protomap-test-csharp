namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyAckItemReportChecks
{
    public static void PolicyAckItemReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709954614);
        var items = new List<PolicyAckItem>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PolicyAckItem.NewPolicyAckItem($"policy-ack-item-{index}", "sample"));
        }
        var report = PolicyAckItemReport.BuildPolicyAckItemReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PolicyAckItemReportCountsEachRecordOnce();
    }
}
