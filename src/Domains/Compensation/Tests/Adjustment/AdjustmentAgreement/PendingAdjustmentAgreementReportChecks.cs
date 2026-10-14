namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class PendingAdjustmentAgreementReportChecks
{
    public static void PendingAdjustmentAgreementReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700400888);
        var items = new List<PendingAdjustmentAgreement>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingAdjustmentAgreement.NewPendingAdjustmentAgreement($"pending-adjustment-agreement-{index}", "sample"));
        }
        var report = PendingAdjustmentAgreementReport.BuildPendingAdjustmentAgreementReport("test", items, clock);
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
        PendingAdjustmentAgreementReportCountsEachRecordOnce();
    }
}
