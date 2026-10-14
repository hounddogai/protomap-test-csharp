namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ExceptionAllocationReportChecks
{
    public static void ExceptionAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708851563);
        var items = new List<ExceptionAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ExceptionAllocation.NewExceptionAllocation($"exception-allocation-{index}", "sample"));
        }
        var report = ExceptionAllocationReport.BuildExceptionAllocationReport("test", items, clock);
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
        ExceptionAllocationReportCountsEachRecordOnce();
    }
}
