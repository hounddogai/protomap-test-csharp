namespace Acme.Workforce.Orgdesign.Tests.CostModel.Policy;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CostModelExceptionReportChecks
{
    public static void CostModelExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708027804);
        var items = new List<CostModelException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CostModelException.NewCostModelException($"cost-model-exception-{index}", "sample"));
        }
        var report = CostModelExceptionReport.BuildCostModelExceptionReport("test", items, clock);
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
        CostModelExceptionReportCountsEachRecordOnce();
    }
}
