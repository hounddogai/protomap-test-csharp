namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceReportChecks
{
    public static void TieredPolicyAckVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703315042);
        var items = new List<TieredPolicyAckVariance>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredPolicyAckVariance.NewTieredPolicyAckVariance($"tiered-policy-ack-variance-{index}", "sample"));
        }
        var report = TieredPolicyAckVarianceReport.BuildTieredPolicyAckVarianceReport("test", items, clock);
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
        TieredPolicyAckVarianceReportCountsEachRecordOnce();
    }
}
