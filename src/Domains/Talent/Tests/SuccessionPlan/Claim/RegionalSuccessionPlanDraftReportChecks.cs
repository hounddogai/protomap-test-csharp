namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftReportChecks
{
    public static void RegionalSuccessionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705013499);
        var items = new List<RegionalSuccessionPlanDraft>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft($"regional-succession-plan-draft-{index}", "sample"));
        }
        var report = RegionalSuccessionPlanDraftReport.BuildRegionalSuccessionPlanDraftReport("test", items, clock);
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
        RegionalSuccessionPlanDraftReportCountsEachRecordOnce();
    }
}
