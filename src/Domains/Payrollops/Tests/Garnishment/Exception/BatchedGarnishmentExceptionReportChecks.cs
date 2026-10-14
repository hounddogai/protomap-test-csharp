namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedGarnishmentExceptionReportChecks
{
    public static void BatchedGarnishmentExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708314374);
        var items = new List<BatchedGarnishmentException>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedGarnishmentException.NewBatchedGarnishmentException($"batched-garnishment-exception-{index}", "sample"));
        }
        var report = BatchedGarnishmentExceptionReport.BuildBatchedGarnishmentExceptionReport("test", items, clock);
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
        BatchedGarnishmentExceptionReportCountsEachRecordOnce();
    }
}
