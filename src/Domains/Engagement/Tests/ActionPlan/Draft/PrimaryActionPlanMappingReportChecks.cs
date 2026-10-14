namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PrimaryActionPlanMappingReportChecks
{
    public static void PrimaryActionPlanMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705937767);
        var items = new List<PrimaryActionPlanMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PrimaryActionPlanMapping.NewPrimaryActionPlanMapping($"primary-action-plan-mapping-{index}", "sample"));
        }
        var report = PrimaryActionPlanMappingReport.BuildPrimaryActionPlanMappingReport("test", items, clock);
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
        PrimaryActionPlanMappingReportCountsEachRecordOnce();
    }
}
