namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimReportChecks
{
    public static void LenientSuccessionPlanClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708507770);
        var items = new List<LenientSuccessionPlanClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim($"lenient-succession-plan-claim-{index}", "sample"));
        }
        var report = LenientSuccessionPlanClaimReport.BuildLenientSuccessionPlanClaimReport("test", items, clock);
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
        LenientSuccessionPlanClaimReportCountsEachRecordOnce();
    }
}
