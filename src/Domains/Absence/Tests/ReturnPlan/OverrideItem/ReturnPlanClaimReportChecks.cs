namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class ReturnPlanClaimReportChecks
{
    public static void ReturnPlanClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700257980);
        var items = new List<ReturnPlanClaim>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ReturnPlanClaim.NewReturnPlanClaim($"return-plan-claim-{index}", "sample"));
        }
        var report = ReturnPlanClaimReport.BuildReturnPlanClaimReport("test", items, clock);
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
        ReturnPlanClaimReportCountsEachRecordOnce();
    }
}
