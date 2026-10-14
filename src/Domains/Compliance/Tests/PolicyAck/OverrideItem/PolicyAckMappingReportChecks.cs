namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckMappingReportChecks
{
    public static void PolicyAckMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707371572);
        var items = new List<PolicyAckMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyAckMapping.NewPolicyAckMapping($"policy-ack-mapping-{index}", "sample"));
        }
        var report = PolicyAckMappingReport.BuildPolicyAckMappingReport("test", items, clock);
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
        PolicyAckMappingReportCountsEachRecordOnce();
    }
}
