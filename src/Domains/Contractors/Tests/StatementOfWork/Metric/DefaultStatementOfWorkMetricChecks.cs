namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Metric;

using Acme.Workforce.Contractors.StatementOfWork.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMetricChecks
{
    public static void DefaultStatementOfWorkMetricStartsInDraft()
    {
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("code-732", "name-210");
        Check.Equal(item.Status, DefaultStatementOfWorkMetricStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultStatementOfWorkMetricMovesToSuspended()
    {
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("code-732", "name-210");
        try
        {
            item.MoveTo(DefaultStatementOfWorkMetricStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultStatementOfWorkMetricStatus.Suspended, "status");
    }

    public static void DefaultStatementOfWorkMetricRejectsMovesFromClosed()
    {
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("code-732", "name-210");
        item.Status = DefaultStatementOfWorkMetricStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultStatementOfWorkMetricStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMetricStartsInDraft();
        DefaultStatementOfWorkMetricMovesToSuspended();
        DefaultStatementOfWorkMetricRejectsMovesFromClosed();
    }
}
