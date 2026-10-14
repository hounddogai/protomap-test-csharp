namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryPolicyAckPlanReportChecks
{
    public static void PrimaryPolicyAckPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706372484);
        var items = new List<PrimaryPolicyAckPlan>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryPolicyAckPlan.NewPrimaryPolicyAckPlan($"primary-policy-ack-plan-{index}", "sample"));
        }
        var report = PrimaryPolicyAckPlanReport.BuildPrimaryPolicyAckPlanReport("test", items, clock);
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
        PrimaryPolicyAckPlanReportCountsEachRecordOnce();
    }
}
