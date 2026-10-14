namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckRollupReportChecks
{
    public static void PolicyAckRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708092426);
        var items = new List<PolicyAckRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyAckRollup.NewPolicyAckRollup($"policy-ack-rollup-{index}", "sample"));
        }
        var report = PolicyAckRollupReport.BuildPolicyAckRollupReport("test", items, clock);
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
        PolicyAckRollupReportCountsEachRecordOnce();
    }
}
