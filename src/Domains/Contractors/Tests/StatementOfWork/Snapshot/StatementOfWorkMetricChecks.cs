namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkMetricChecks
{
    public static void StatementOfWorkMetricStartsInNew()
    {
        var item = StatementOfWorkMetric.NewStatementOfWorkMetric("code-737", "title-546");
        Check.Equal(item.Status, StatementOfWorkMetricStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkMetricMovesToDone()
    {
        var item = StatementOfWorkMetric.NewStatementOfWorkMetric("code-737", "title-546");
        try
        {
            item.MoveTo(StatementOfWorkMetricStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkMetricStatus.Done, "status");
    }

    public static void StatementOfWorkMetricRejectsMovesFromFailed()
    {
        var item = StatementOfWorkMetric.NewStatementOfWorkMetric("code-737", "title-546");
        item.Status = StatementOfWorkMetricStatus.Failed;
        Check.Fails(() => item.MoveTo(StatementOfWorkMetricStatus.New), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkMetricStartsInNew();
        StatementOfWorkMetricMovesToDone();
        StatementOfWorkMetricRejectsMovesFromFailed();
    }
}
