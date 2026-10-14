namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowReportChecks
{
    public static void SuccessionPlanWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708984011);
        var items = new List<SuccessionPlanWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SuccessionPlanWindow.NewSuccessionPlanWindow($"succession-plan-window-{index}", "sample"));
        }
        var report = SuccessionPlanWindowReport.BuildSuccessionPlanWindowReport("test", items, clock);
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
        SuccessionPlanWindowReportCountsEachRecordOnce();
    }
}
