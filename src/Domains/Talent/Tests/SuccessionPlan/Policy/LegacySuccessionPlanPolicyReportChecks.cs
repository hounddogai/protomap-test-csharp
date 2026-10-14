namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LegacySuccessionPlanPolicyReportChecks
{
    public static void LegacySuccessionPlanPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702873206);
        var items = new List<LegacySuccessionPlanPolicy>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacySuccessionPlanPolicy.NewLegacySuccessionPlanPolicy($"legacy-succession-plan-policy-{index}", "sample"));
        }
        var report = LegacySuccessionPlanPolicyReport.BuildLegacySuccessionPlanPolicyReport("test", items, clock);
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
        LegacySuccessionPlanPolicyReportCountsEachRecordOnce();
    }
}
