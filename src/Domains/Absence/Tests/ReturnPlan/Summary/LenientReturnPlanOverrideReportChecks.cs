namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientReturnPlanOverrideReportChecks
{
    public static void LenientReturnPlanOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704263497);
        var items = new List<LenientReturnPlanOverride>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientReturnPlanOverride.NewLenientReturnPlanOverride($"lenient-return-plan-override-{index}", "sample"));
        }
        var report = LenientReturnPlanOverrideReport.BuildLenientReturnPlanOverrideReport("test", items, clock);
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
        LenientReturnPlanOverrideReportCountsEachRecordOnce();
    }
}
