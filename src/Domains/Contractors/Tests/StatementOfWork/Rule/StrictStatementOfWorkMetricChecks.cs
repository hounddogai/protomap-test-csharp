namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkMetricChecks
{
    public static void StrictStatementOfWorkMetricStartsInProposed()
    {
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("code-137", "name-494");
        Check.Equal(item.Status, StrictStatementOfWorkMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictStatementOfWorkMetricMovesToRejected()
    {
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("code-137", "name-494");
        try
        {
            item.MoveTo(StrictStatementOfWorkMetricStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictStatementOfWorkMetricStatus.Rejected, "status");
    }

    public static void StrictStatementOfWorkMetricRejectsMovesFromWithdrawn()
    {
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("code-137", "name-494");
        item.Status = StrictStatementOfWorkMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictStatementOfWorkMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkMetricStartsInProposed();
        StrictStatementOfWorkMetricMovesToRejected();
        StrictStatementOfWorkMetricRejectsMovesFromWithdrawn();
    }
}
