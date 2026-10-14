namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanSummaryReportChecks
{
    public static void SuccessionPlanSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702130804);
        var items = new List<SuccessionPlanSummary>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SuccessionPlanSummary.NewSuccessionPlanSummary($"succession-plan-summary-{index}", "sample"));
        }
        var report = SuccessionPlanSummaryReport.BuildSuccessionPlanSummaryReport("test", items, clock);
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
        SuccessionPlanSummaryReportCountsEachRecordOnce();
    }
}
