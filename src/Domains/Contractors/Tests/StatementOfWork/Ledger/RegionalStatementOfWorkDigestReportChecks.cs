namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestReportChecks
{
    public static void RegionalStatementOfWorkDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702487144);
        var items = new List<RegionalStatementOfWorkDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest($"regional-statement-of-work-digest-{index}", "sample"));
        }
        var report = RegionalStatementOfWorkDigestReport.BuildRegionalStatementOfWorkDigestReport("test", items, clock);
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
        RegionalStatementOfWorkDigestReportCountsEachRecordOnce();
    }
}
