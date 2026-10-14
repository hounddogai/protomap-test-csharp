namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultPayslipReviewReportChecks
{
    public static void DefaultPayslipReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707182301);
        var items = new List<DefaultPayslipReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(DefaultPayslipReview.NewDefaultPayslipReview($"default-payslip-review-{index}", "sample"));
        }
        var report = DefaultPayslipReviewReport.BuildDefaultPayslipReviewReport("test", items, clock);
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
        DefaultPayslipReviewReportCountsEachRecordOnce();
    }
}
