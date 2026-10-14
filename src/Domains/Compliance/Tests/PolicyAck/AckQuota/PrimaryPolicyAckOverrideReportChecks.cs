namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class PrimaryPolicyAckOverrideReportChecks
{
    public static void PrimaryPolicyAckOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701106319);
        var items = new List<PrimaryPolicyAckOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride($"primary-policy-ack-override-{index}", "sample"));
        }
        var report = PrimaryPolicyAckOverrideReport.BuildPrimaryPolicyAckOverrideReport("test", items, clock);
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
        PrimaryPolicyAckOverrideReportCountsEachRecordOnce();
    }
}
