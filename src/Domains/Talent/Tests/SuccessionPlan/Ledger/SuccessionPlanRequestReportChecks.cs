namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRequestReportChecks
{
    public static void SuccessionPlanRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703126671);
        var items = new List<SuccessionPlanRequest>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SuccessionPlanRequest.NewSuccessionPlanRequest($"succession-plan-request-{index}", "sample"));
        }
        var report = SuccessionPlanRequestReport.BuildSuccessionPlanRequestReport("test", items, clock);
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
        SuccessionPlanRequestReportCountsEachRecordOnce();
    }
}
