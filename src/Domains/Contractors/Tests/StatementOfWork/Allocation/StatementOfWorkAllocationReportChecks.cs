namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAllocationReportChecks
{
    public static void StatementOfWorkAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709376183);
        var items = new List<StatementOfWorkAllocation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StatementOfWorkAllocation.NewStatementOfWorkAllocation($"statement-of-work-allocation-{index}", "sample"));
        }
        var report = StatementOfWorkAllocationReport.BuildStatementOfWorkAllocationReport("test", items, clock);
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
        StatementOfWorkAllocationReportCountsEachRecordOnce();
    }
}
