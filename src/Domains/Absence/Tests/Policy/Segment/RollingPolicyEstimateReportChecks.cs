namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingPolicyEstimateReportChecks
{
    public static void RollingPolicyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705457455);
        var items = new List<RollingPolicyEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingPolicyEstimate.NewRollingPolicyEstimate($"rolling-policy-estimate-{index}", "sample"));
        }
        var report = RollingPolicyEstimateReport.BuildRollingPolicyEstimateReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RollingPolicyEstimateReportCountsEachRecordOnce();
    }
}
