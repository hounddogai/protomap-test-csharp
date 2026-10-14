namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class StatementOfWorkDraftReportChecks
{
    public static void StatementOfWorkDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700165255);
        var items = new List<StatementOfWorkDraft>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(StatementOfWorkDraft.NewStatementOfWorkDraft($"statement-of-work-draft-{index}", "sample"));
        }
        var report = StatementOfWorkDraftReport.BuildStatementOfWorkDraftReport("test", items, clock);
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
        StatementOfWorkDraftReportCountsEachRecordOnce();
    }
}
