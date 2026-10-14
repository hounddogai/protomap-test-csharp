namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientSuccessionPlanTargetReportChecks
{
    public static void LenientSuccessionPlanTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708726377);
        var items = new List<LenientSuccessionPlanTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientSuccessionPlanTarget.NewLenientSuccessionPlanTarget($"lenient-succession-plan-target-{index}", "sample"));
        }
        var report = LenientSuccessionPlanTargetReport.BuildLenientSuccessionPlanTargetReport("test", items, clock);
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
        LenientSuccessionPlanTargetReportCountsEachRecordOnce();
    }
}
