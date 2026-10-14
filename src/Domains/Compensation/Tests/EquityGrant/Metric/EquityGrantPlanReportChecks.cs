namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantPlanReportChecks
{
    public static void EquityGrantPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706552542);
        var items = new List<EquityGrantPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EquityGrantPlan.NewEquityGrantPlan($"equity-grant-plan-{index}", "sample"));
        }
        var report = EquityGrantPlanReport.BuildEquityGrantPlanReport("test", items, clock);
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
        EquityGrantPlanReportCountsEachRecordOnce();
    }
}
