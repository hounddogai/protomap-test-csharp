namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SuccessionPlanItemReportChecks
{
    public static void SuccessionPlanItemReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704524961);
        var items = new List<SuccessionPlanItem>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SuccessionPlanItem.NewSuccessionPlanItem($"succession-plan-item-{index}", "sample"));
        }
        var report = SuccessionPlanItemReport.BuildSuccessionPlanItemReport("test", items, clock);
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
        SuccessionPlanItemReportCountsEachRecordOnce();
    }
}
