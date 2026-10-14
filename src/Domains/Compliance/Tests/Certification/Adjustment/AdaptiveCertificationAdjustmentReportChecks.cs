namespace Acme.Workforce.Compliance.Tests.Certification.Adjustment;

using Acme.Workforce.Compliance.Certification.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AdaptiveCertificationAdjustmentReportChecks
{
    public static void AdaptiveCertificationAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706224782);
        var items = new List<AdaptiveCertificationAdjustment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveCertificationAdjustment.NewAdaptiveCertificationAdjustment($"adaptive-certification-adjustment-{index}", "sample"));
        }
        var report = AdaptiveCertificationAdjustmentReport.BuildAdaptiveCertificationAdjustmentReport("test", items, clock);
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
        AdaptiveCertificationAdjustmentReportCountsEachRecordOnce();
    }
}
