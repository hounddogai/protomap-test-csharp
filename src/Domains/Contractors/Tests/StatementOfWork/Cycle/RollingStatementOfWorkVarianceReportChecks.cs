namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingStatementOfWorkVarianceReportChecks
{
    public static void RollingStatementOfWorkVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704297136);
        var items = new List<RollingStatementOfWorkVariance>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance($"rolling-statement-of-work-variance-{index}", "sample"));
        }
        var report = RollingStatementOfWorkVarianceReport.BuildRollingStatementOfWorkVarianceReport("test", items, clock);
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
        RollingStatementOfWorkVarianceReportCountsEachRecordOnce();
    }
}
