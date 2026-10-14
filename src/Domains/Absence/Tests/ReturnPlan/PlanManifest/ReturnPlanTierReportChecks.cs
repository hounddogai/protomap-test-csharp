namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierReportChecks
{
    public static void ReturnPlanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702521858);
        var items = new List<ReturnPlanTier>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ReturnPlanTier.NewReturnPlanTier($"return-plan-tier-{index}", "sample"));
        }
        var report = ReturnPlanTierReport.BuildReturnPlanTierReport("test", items, clock);
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
        ReturnPlanTierReportCountsEachRecordOnce();
    }
}
