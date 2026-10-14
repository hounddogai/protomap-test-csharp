namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackPolicyAckRequestReportChecks
{
    public static void FallbackPolicyAckRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701130442);
        var items = new List<FallbackPolicyAckRequest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(FallbackPolicyAckRequest.NewFallbackPolicyAckRequest($"fallback-policy-ack-request-{index}", "sample"));
        }
        var report = FallbackPolicyAckRequestReport.BuildFallbackPolicyAckRequestReport("test", items, clock);
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
        FallbackPolicyAckRequestReportCountsEachRecordOnce();
    }
}
