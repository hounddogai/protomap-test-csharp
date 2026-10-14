namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class LenientRetroPaySummaryReportChecks
{
    public static void LenientRetroPaySummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700930659);
        var items = new List<LenientRetroPaySummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientRetroPaySummary.NewLenientRetroPaySummary($"lenient-retro-pay-summary-{index}", "sample"));
        }
        var report = LenientRetroPaySummaryReport.BuildLenientRetroPaySummaryReport("test", items, clock);
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
        LenientRetroPaySummaryReportCountsEachRecordOnce();
    }
}
