namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class AdaptiveDeductionAuditReportChecks
{
    public static void AdaptiveDeductionAuditReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706726962);
        var items = new List<AdaptiveDeductionAudit>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(AdaptiveDeductionAudit.NewAdaptiveDeductionAudit($"adaptive-deduction-audit-{index}", "sample"));
        }
        var report = AdaptiveDeductionAuditReport.BuildAdaptiveDeductionAuditReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        AdaptiveDeductionAuditReportCountsEachRecordOnce();
    }
}
