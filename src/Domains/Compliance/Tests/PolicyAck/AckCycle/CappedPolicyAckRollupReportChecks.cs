namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupReportChecks
{
    public static void CappedPolicyAckRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700645618);
        var items = new List<CappedPolicyAckRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedPolicyAckRollup.NewCappedPolicyAckRollup($"capped-policy-ack-rollup-{index}", "sample"));
        }
        var report = CappedPolicyAckRollupReport.BuildCappedPolicyAckRollupReport("test", items, clock);
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
        CappedPolicyAckRollupReportCountsEachRecordOnce();
    }
}
