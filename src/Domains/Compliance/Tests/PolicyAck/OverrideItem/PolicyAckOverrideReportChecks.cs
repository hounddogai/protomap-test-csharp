namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideReportChecks
{
    public static void PolicyAckOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707654967);
        var items = new List<PolicyAckOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PolicyAckOverride.NewPolicyAckOverride($"policy-ack-override-{index}", "sample"));
        }
        var report = PolicyAckOverrideReport.BuildPolicyAckOverrideReport("test", items, clock);
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
        PolicyAckOverrideReportCountsEachRecordOnce();
    }
}
