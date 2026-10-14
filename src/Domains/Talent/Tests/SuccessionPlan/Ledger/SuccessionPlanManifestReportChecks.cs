namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanManifestReportChecks
{
    public static void SuccessionPlanManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705049510);
        var items = new List<SuccessionPlanManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SuccessionPlanManifest.NewSuccessionPlanManifest($"succession-plan-manifest-{index}", "sample"));
        }
        var report = SuccessionPlanManifestReport.BuildSuccessionPlanManifestReport("test", items, clock);
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
        SuccessionPlanManifestReportCountsEachRecordOnce();
    }
}
