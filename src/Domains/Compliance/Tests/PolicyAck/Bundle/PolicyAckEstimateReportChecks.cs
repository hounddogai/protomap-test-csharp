namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckEstimateReportChecks
{
    public static void PolicyAckEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706735980);
        var items = new List<PolicyAckEstimate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PolicyAckEstimate.NewPolicyAckEstimate($"policy-ack-estimate-{index}", "sample"));
        }
        var report = PolicyAckEstimateReport.BuildPolicyAckEstimateReport("test", items, clock);
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
        PolicyAckEstimateReportCountsEachRecordOnce();
    }
}
