namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryStatementOfWorkAllocationReportChecks
{
    public static void PrimaryStatementOfWorkAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709705081);
        var items = new List<PrimaryStatementOfWorkAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation($"primary-statement-of-work-allocation-{index}", "sample"));
        }
        var report = PrimaryStatementOfWorkAllocationReport.BuildPrimaryStatementOfWorkAllocationReport("test", items, clock);
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
        PrimaryStatementOfWorkAllocationReportCountsEachRecordOnce();
    }
}
