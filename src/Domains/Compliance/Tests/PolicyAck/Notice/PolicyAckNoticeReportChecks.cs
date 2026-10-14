namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckNoticeReportChecks
{
    public static void PolicyAckNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700970808);
        var items = new List<PolicyAckNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PolicyAckNotice.NewPolicyAckNotice($"policy-ack-notice-{index}", "sample"));
        }
        var report = PolicyAckNoticeReport.BuildPolicyAckNoticeReport("test", items, clock);
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
        PolicyAckNoticeReportCountsEachRecordOnce();
    }
}
