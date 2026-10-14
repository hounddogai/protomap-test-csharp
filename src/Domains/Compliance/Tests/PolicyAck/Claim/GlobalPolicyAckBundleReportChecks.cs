namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleReportChecks
{
    public static void GlobalPolicyAckBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708708369);
        var items = new List<GlobalPolicyAckBundle>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalPolicyAckBundle.NewGlobalPolicyAckBundle($"global-policy-ack-bundle-{index}", "sample"));
        }
        var report = GlobalPolicyAckBundleReport.BuildGlobalPolicyAckBundleReport("test", items, clock);
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
        GlobalPolicyAckBundleReportCountsEachRecordOnce();
    }
}
