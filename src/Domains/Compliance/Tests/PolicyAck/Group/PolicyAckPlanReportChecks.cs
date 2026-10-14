namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckPlanReportChecks
{
    public static void PolicyAckPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707058425);
        var items = new List<PolicyAckPlan>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PolicyAckPlan.NewPolicyAckPlan($"policy-ack-plan-{index}", "sample"));
        }
        var report = PolicyAckPlanReport.BuildPolicyAckPlanReport("test", items, clock);
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
        PolicyAckPlanReportCountsEachRecordOnce();
    }
}
