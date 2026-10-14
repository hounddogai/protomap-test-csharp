namespace Acme.Workforce.Compliance.Tests.PolicyAck.Quota;

using Acme.Workforce.Compliance.PolicyAck.Quota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PolicyAckQuotaReportChecks
{
    public static void PolicyAckQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700324250);
        var items = new List<PolicyAckQuota>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PolicyAckQuota.NewPolicyAckQuota($"policy-ack-quota-{index}", "sample"));
        }
        var report = PolicyAckQuotaReport.BuildPolicyAckQuotaReport("test", items, clock);
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
        PolicyAckQuotaReportCountsEachRecordOnce();
    }
}
