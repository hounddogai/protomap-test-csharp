namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunEntryReportChecks
{
    public static void PayRunEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705239715);
        var items = new List<PayRunEntry>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PayRunEntry.NewPayRunEntry($"pay-run-entry-{index}", "sample"));
        }
        var report = PayRunEntryReport.BuildPayRunEntryReport("test", items, clock);
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
        PayRunEntryReportCountsEachRecordOnce();
    }
}
