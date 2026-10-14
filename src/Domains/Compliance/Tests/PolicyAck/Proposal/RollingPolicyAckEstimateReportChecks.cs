namespace Acme.Workforce.Compliance.Tests.PolicyAck.Proposal;

using Acme.Workforce.Compliance.PolicyAck.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingPolicyAckEstimateReportChecks
{
    public static void RollingPolicyAckEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703419278);
        var items = new List<RollingPolicyAckEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingPolicyAckEstimate.NewRollingPolicyAckEstimate($"rolling-policy-ack-estimate-{index}", "sample"));
        }
        var report = RollingPolicyAckEstimateReport.BuildRollingPolicyAckEstimateReport("test", items, clock);
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
        RollingPolicyAckEstimateReportCountsEachRecordOnce();
    }
}
