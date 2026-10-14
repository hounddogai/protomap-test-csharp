namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class PrimaryReturnPlanManifestReportChecks
{
    public static void PrimaryReturnPlanManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709035766);
        var items = new List<PrimaryReturnPlanManifest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest($"primary-return-plan-manifest-{index}", "sample"));
        }
        var report = PrimaryReturnPlanManifestReport.BuildPrimaryReturnPlanManifestReport("test", items, clock);
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
        PrimaryReturnPlanManifestReportCountsEachRecordOnce();
    }
}
