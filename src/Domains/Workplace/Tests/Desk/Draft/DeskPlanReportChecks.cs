namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DeskPlanReportChecks
{
    public static void DeskPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700653556);
        var items = new List<DeskPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DeskPlan.NewDeskPlan($"desk-plan-{index}", "sample"));
        }
        var report = DeskPlanReport.BuildDeskPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DeskPlanReportCountsEachRecordOnce();
    }
}
