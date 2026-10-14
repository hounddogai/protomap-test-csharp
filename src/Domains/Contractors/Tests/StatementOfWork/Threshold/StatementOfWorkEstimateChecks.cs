namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkEstimateChecks
{
    public static void StatementOfWorkEstimateStartsInDraft()
    {
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
        Check.Equal(item.Status, StatementOfWorkEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkEstimateMovesToSuspended()
    {
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
        try
        {
            item.MoveTo(StatementOfWorkEstimateStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkEstimateStatus.Suspended, "status");
    }

    public static void StatementOfWorkEstimateRejectsMovesFromClosed()
    {
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
        item.Status = StatementOfWorkEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkEstimateStartsInDraft();
        StatementOfWorkEstimateMovesToSuspended();
        StatementOfWorkEstimateRejectsMovesFromClosed();
    }
}
