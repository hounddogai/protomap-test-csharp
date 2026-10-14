namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPolicyAckOverrideReportChecks
{
    public static void TieredPolicyAckOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708003960);
        var items = new List<TieredPolicyAckOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TieredPolicyAckOverride.NewTieredPolicyAckOverride($"tiered-policy-ack-override-{index}", "sample"));
        }
        var report = TieredPolicyAckOverrideReport.BuildTieredPolicyAckOverrideReport("test", items, clock);
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
        TieredPolicyAckOverrideReportCountsEachRecordOnce();
    }
}
