namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanQuotaReportChecks
{
    public static void GlobalSuccessionPlanQuotaReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701738760);
        var items = new List<GlobalSuccessionPlanQuota>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalSuccessionPlanQuota.NewGlobalSuccessionPlanQuota($"global-succession-plan-quota-{index}", "sample"));
        }
        var report = GlobalSuccessionPlanQuotaReport.BuildGlobalSuccessionPlanQuotaReport("test", items, clock);
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
        GlobalSuccessionPlanQuotaReportCountsEachRecordOnce();
    }
}
