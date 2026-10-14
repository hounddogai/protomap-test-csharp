namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientActionPlanBundleReportChecks
{
    public static void LenientActionPlanBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702820746);
        var items = new List<LenientActionPlanBundle>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LenientActionPlanBundle.NewLenientActionPlanBundle($"lenient-action-plan-bundle-{index}", "sample"));
        }
        var report = LenientActionPlanBundleReport.BuildLenientActionPlanBundleReport("test", items, clock);
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
        LenientActionPlanBundleReportCountsEachRecordOnce();
    }
}
