namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class DefaultExceptionAllocationReportChecks
{
    public static void DefaultExceptionAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704204185);
        var items = new List<DefaultExceptionAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(DefaultExceptionAllocation.NewDefaultExceptionAllocation($"default-exception-allocation-{index}", "sample"));
        }
        var report = DefaultExceptionAllocationReport.BuildDefaultExceptionAllocationReport("test", items, clock);
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
        DefaultExceptionAllocationReportCountsEachRecordOnce();
    }
}
