namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalReportChecks
{
    public static void CappedStatementOfWorkProposalReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707978706);
        var items = new List<CappedStatementOfWorkProposal>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal($"capped-statement-of-work-proposal-{index}", "sample"));
        }
        var report = CappedStatementOfWorkProposalReport.BuildCappedStatementOfWorkProposalReport("test", items, clock);
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
        CappedStatementOfWorkProposalReportCountsEachRecordOnce();
    }
}
