namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultAdjustmentAgreementReportChecks
{
    public static void DefaultAdjustmentAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706592250);
        var items = new List<DefaultAdjustmentAgreement>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultAdjustmentAgreement.NewDefaultAdjustmentAgreement($"default-adjustment-agreement-{index}", "sample"));
        }
        var report = DefaultAdjustmentAgreementReport.BuildDefaultAdjustmentAgreementReport("test", items, clock);
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
        DefaultAdjustmentAgreementReportCountsEachRecordOnce();
    }
}
