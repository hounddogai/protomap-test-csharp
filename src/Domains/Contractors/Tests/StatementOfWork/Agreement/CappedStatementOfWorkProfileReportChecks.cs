namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProfileReportChecks
{
    public static void CappedStatementOfWorkProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707510041);
        var items = new List<CappedStatementOfWorkProfile>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile($"capped-statement-of-work-profile-{index}", "sample"));
        }
        var report = CappedStatementOfWorkProfileReport.BuildCappedStatementOfWorkProfileReport("test", items, clock);
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
        CappedStatementOfWorkProfileReportCountsEachRecordOnce();
    }
}
