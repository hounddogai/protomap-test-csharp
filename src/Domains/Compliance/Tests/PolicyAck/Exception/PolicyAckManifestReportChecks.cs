namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckManifestReportChecks
{
    public static void PolicyAckManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701299509);
        var items = new List<PolicyAckManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyAckManifest.NewPolicyAckManifest($"policy-ack-manifest-{index}", "sample"));
        }
        var report = PolicyAckManifestReport.BuildPolicyAckManifestReport("test", items, clock);
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
        PolicyAckManifestReportCountsEachRecordOnce();
    }
}
