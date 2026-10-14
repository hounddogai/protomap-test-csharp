namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimReportChecks
{
    public static void AdaptivePolicyAckClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701965372);
        var items = new List<AdaptivePolicyAckClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim($"adaptive-policy-ack-claim-{index}", "sample"));
        }
        var report = AdaptivePolicyAckClaimReport.BuildAdaptivePolicyAckClaimReport("test", items, clock);
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
        AdaptivePolicyAckClaimReportCountsEachRecordOnce();
    }
}
