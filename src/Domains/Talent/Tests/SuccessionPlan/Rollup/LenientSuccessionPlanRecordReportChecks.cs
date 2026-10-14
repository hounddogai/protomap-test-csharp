namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientSuccessionPlanRecordReportChecks
{
    public static void LenientSuccessionPlanRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709767954);
        var items = new List<LenientSuccessionPlanRecord>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientSuccessionPlanRecord.NewLenientSuccessionPlanRecord($"lenient-succession-plan-record-{index}", "sample"));
        }
        var report = LenientSuccessionPlanRecordReport.BuildLenientSuccessionPlanRecordReport("test", items, clock);
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
        LenientSuccessionPlanRecordReportCountsEachRecordOnce();
    }
}
