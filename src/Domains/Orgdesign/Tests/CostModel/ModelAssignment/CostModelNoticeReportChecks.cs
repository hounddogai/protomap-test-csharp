namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelNoticeReportChecks
{
    public static void CostModelNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709909165);
        var items = new List<CostModelNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(CostModelNotice.NewCostModelNotice($"cost-model-notice-{index}", "sample"));
        }
        var report = CostModelNoticeReport.BuildCostModelNoticeReport("test", items, clock);
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
        CostModelNoticeReportCountsEachRecordOnce();
    }
}
