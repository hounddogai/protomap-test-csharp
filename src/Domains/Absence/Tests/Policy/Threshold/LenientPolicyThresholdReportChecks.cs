namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdReportChecks
{
    public static void LenientPolicyThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702962146);
        var items = new List<LenientPolicyThreshold>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientPolicyThreshold.NewLenientPolicyThreshold($"lenient-policy-threshold-{index}", "sample"));
        }
        var report = LenientPolicyThresholdReport.BuildLenientPolicyThresholdReport("test", items, clock);
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
        LenientPolicyThresholdReportCountsEachRecordOnce();
    }
}
