namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackCostModelRequestReportChecks
{
    public static void FallbackCostModelRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707013187);
        var items = new List<FallbackCostModelRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackCostModelRequest.NewFallbackCostModelRequest($"fallback-cost-model-request-{index}", "sample"));
        }
        var report = FallbackCostModelRequestReport.BuildFallbackCostModelRequestReport("test", items, clock);
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
        FallbackCostModelRequestReportCountsEachRecordOnce();
    }
}
