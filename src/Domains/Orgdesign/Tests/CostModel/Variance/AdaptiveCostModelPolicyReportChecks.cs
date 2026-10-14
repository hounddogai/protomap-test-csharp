namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveCostModelPolicyReportChecks
{
    public static void AdaptiveCostModelPolicyReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703187640);
        var items = new List<AdaptiveCostModelPolicy>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy($"adaptive-cost-model-policy-{index}", "sample"));
        }
        var report = AdaptiveCostModelPolicyReport.BuildAdaptiveCostModelPolicyReport("test", items, clock);
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
        AdaptiveCostModelPolicyReportCountsEachRecordOnce();
    }
}
