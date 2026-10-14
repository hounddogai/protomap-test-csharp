namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SuccessionPlanPolicyReportChecks
{
    public static void SuccessionPlanPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704151305);
        var items = new List<SuccessionPlanPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SuccessionPlanPolicy.NewSuccessionPlanPolicy($"succession-plan-policy-{index}", "sample"));
        }
        var report = SuccessionPlanPolicyReport.BuildSuccessionPlanPolicyReport("test", items, clock);
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
        SuccessionPlanPolicyReportCountsEachRecordOnce();
    }
}
