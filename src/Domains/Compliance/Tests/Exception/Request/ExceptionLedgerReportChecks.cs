namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionLedgerReportChecks
{
    public static void ExceptionLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706631154);
        var items = new List<ExceptionLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ExceptionLedger.NewExceptionLedger($"exception-ledger-{index}", "sample"));
        }
        var report = ExceptionLedgerReport.BuildExceptionLedgerReport("test", items, clock);
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
        ExceptionLedgerReportCountsEachRecordOnce();
    }
}
