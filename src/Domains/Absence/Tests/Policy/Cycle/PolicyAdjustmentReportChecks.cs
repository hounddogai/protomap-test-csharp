namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyAdjustmentReportChecks
{
    public static void PolicyAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709044922);
        var items = new List<PolicyAdjustment>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PolicyAdjustment.NewPolicyAdjustment($"policy-adjustment-{index}", "sample"));
        }
        var report = PolicyAdjustmentReport.BuildPolicyAdjustmentReport("test", items, clock);
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
        PolicyAdjustmentReportCountsEachRecordOnce();
    }
}
