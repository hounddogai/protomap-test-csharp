namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunExceptionReportChecks
{
    public static void PayRunExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709365578);
        var items = new List<PayRunException>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PayRunException.NewPayRunException($"pay-run-exception-{index}", "sample"));
        }
        var report = PayRunExceptionReport.BuildPayRunExceptionReport("test", items, clock);
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
        PayRunExceptionReportCountsEachRecordOnce();
    }
}
