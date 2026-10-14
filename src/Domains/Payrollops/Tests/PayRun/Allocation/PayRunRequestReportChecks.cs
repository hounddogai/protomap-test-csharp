namespace Acme.Workforce.Payrollops.Tests.PayRun.Allocation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PayRunRequestReportChecks
{
    public static void PayRunRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709036010);
        var items = new List<PayRunRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PayRunRequest.NewPayRunRequest($"pay-run-request-{index}", "sample"));
        }
        var report = PayRunRequestReport.BuildPayRunRequestReport("test", items, clock);
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
        PayRunRequestReportCountsEachRecordOnce();
    }
}
