namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanTemplateReportChecks
{
    public static void FloorPlanTemplateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704782065);
        var items = new List<FloorPlanTemplate>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FloorPlanTemplate.NewFloorPlanTemplate($"floor-plan-template-{index}", "sample"));
        }
        var report = FloorPlanTemplateReport.BuildFloorPlanTemplateReport("test", items, clock);
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
        FloorPlanTemplateReportCountsEachRecordOnce();
    }
}
