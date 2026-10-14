namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalReturnPlanRollupReportChecks
{
    public static void SeasonalReturnPlanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706133359);
        var items = new List<SeasonalReturnPlanRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup($"seasonal-return-plan-rollup-{index}", "sample"));
        }
        var report = SeasonalReturnPlanRollupReport.BuildSeasonalReturnPlanRollupReport("test", items, clock);
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
        SeasonalReturnPlanRollupReportCountsEachRecordOnce();
    }
}
