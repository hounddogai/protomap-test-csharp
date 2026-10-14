namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class LegacyRetroPaySummaryReportChecks
{
    public static void LegacyRetroPaySummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707637748);
        var items = new List<LegacyRetroPaySummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LegacyRetroPaySummary.NewLegacyRetroPaySummary($"legacy-retro-pay-summary-{index}", "sample"));
        }
        var report = LegacyRetroPaySummaryReport.BuildLegacyRetroPaySummaryReport("test", items, clock);
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
        LegacyRetroPaySummaryReportCountsEachRecordOnce();
    }
}
