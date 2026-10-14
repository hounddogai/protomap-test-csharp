namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkProfileReportChecks
{
    public static void StatementOfWorkProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702819763);
        var items = new List<StatementOfWorkProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StatementOfWorkProfile.NewStatementOfWorkProfile($"statement-of-work-profile-{index}", "sample"));
        }
        var report = StatementOfWorkProfileReport.BuildStatementOfWorkProfileReport("test", items, clock);
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
        StatementOfWorkProfileReportCountsEachRecordOnce();
    }
}
