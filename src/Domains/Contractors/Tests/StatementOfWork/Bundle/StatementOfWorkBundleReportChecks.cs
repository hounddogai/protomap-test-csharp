namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleReportChecks
{
    public static void StatementOfWorkBundleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706374630);
        var items = new List<StatementOfWorkBundle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StatementOfWorkBundle.NewStatementOfWorkBundle($"statement-of-work-bundle-{index}", "sample"));
        }
        var report = StatementOfWorkBundleReport.BuildStatementOfWorkBundleReport("test", items, clock);
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
        StatementOfWorkBundleReportCountsEachRecordOnce();
    }
}
