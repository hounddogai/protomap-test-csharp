namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkTargetChecks
{
    public static void StatementOfWorkTargetStartsInPlanned()
    {
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("code-452", "title-660");
        Check.Equal(item.Status, StatementOfWorkTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkTargetMovesToCompleted()
    {
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("code-452", "title-660");
        try
        {
            item.MoveTo(StatementOfWorkTargetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkTargetStatus.Completed, "status");
    }

    public static void StatementOfWorkTargetRejectsMovesFromCancelled()
    {
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("code-452", "title-660");
        item.Status = StatementOfWorkTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StatementOfWorkTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkTargetStartsInPlanned();
        StatementOfWorkTargetMovesToCompleted();
        StatementOfWorkTargetRejectsMovesFromCancelled();
    }
}
