namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class PendingCostModelPolicyReportChecks
{
    public static void PendingCostModelPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705727039);
        var items = new List<PendingCostModelPolicy>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingCostModelPolicy.NewPendingCostModelPolicy($"pending-cost-model-policy-{index}", "sample"));
        }
        var report = PendingCostModelPolicyReport.BuildPendingCostModelPolicyReport("test", items, clock);
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
        PendingCostModelPolicyReportCountsEachRecordOnce();
    }
}
