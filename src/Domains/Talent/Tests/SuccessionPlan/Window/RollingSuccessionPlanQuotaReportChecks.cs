namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingSuccessionPlanQuotaReportChecks
{
    public static void RollingSuccessionPlanQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702507435);
        var items = new List<RollingSuccessionPlanQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota($"rolling-succession-plan-quota-{index}", "sample"));
        }
        var report = RollingSuccessionPlanQuotaReport.BuildRollingSuccessionPlanQuotaReport("test", items, clock);
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
        RollingSuccessionPlanQuotaReportCountsEachRecordOnce();
    }
}
