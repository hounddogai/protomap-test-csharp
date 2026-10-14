namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class ScheduledCostModelPolicyReportChecks
{
    public static void ScheduledCostModelPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701226306);
        var items = new List<ScheduledCostModelPolicy>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledCostModelPolicy.NewScheduledCostModelPolicy($"scheduled-cost-model-policy-{index}", "sample"));
        }
        var report = ScheduledCostModelPolicyReport.BuildScheduledCostModelPolicyReport("test", items, clock);
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
        ScheduledCostModelPolicyReportCountsEachRecordOnce();
    }
}
