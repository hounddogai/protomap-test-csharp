namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictReturnPlanBundleReportChecks
{
    public static void StrictReturnPlanBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702351144);
        var items = new List<StrictReturnPlanBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(StrictReturnPlanBundle.NewStrictReturnPlanBundle($"strict-return-plan-bundle-{index}", "sample"));
        }
        var report = StrictReturnPlanBundleReport.BuildStrictReturnPlanBundleReport("test", items, clock);
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
        StrictReturnPlanBundleReportCountsEachRecordOnce();
    }
}
