namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineChecks
{
    public static void StatementOfWorkBaselineStartsInCreated()
    {
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
        Check.Equal(item.Status, StatementOfWorkBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkBaselineMovesToPublished()
    {
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
        try
        {
            item.MoveTo(StatementOfWorkBaselineStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkBaselineStatus.Published, "status");
    }

    public static void StatementOfWorkBaselineRejectsMovesFromRetired()
    {
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
        item.Status = StatementOfWorkBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkBaselineStartsInCreated();
        StatementOfWorkBaselineMovesToPublished();
        StatementOfWorkBaselineRejectsMovesFromRetired();
    }
}
