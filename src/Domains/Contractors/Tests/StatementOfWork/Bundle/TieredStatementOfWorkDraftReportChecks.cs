namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredStatementOfWorkDraftReportChecks
{
    public static void TieredStatementOfWorkDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706342706);
        var items = new List<TieredStatementOfWorkDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredStatementOfWorkDraft.NewTieredStatementOfWorkDraft($"tiered-statement-of-work-draft-{index}", "sample"));
        }
        var report = TieredStatementOfWorkDraftReport.BuildTieredStatementOfWorkDraftReport("test", items, clock);
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
        TieredStatementOfWorkDraftReportCountsEachRecordOnce();
    }
}
