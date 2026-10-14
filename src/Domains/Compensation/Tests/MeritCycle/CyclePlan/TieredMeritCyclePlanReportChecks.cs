namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class TieredMeritCyclePlanReportChecks
{
    public static void TieredMeritCyclePlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700521137);
        var items = new List<TieredMeritCyclePlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredMeritCyclePlan.NewTieredMeritCyclePlan($"tiered-merit-cycle-plan-{index}", "sample"));
        }
        var report = TieredMeritCyclePlanReport.BuildTieredMeritCyclePlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredMeritCyclePlanReportCountsEachRecordOnce();
    }
}
