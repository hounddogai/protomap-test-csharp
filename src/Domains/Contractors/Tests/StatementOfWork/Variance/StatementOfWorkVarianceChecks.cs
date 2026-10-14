namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkVarianceChecks
{
    public static void StatementOfWorkVarianceStartsInCreated()
    {
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("code-1", "title-662");
        Check.Equal(item.Status, StatementOfWorkVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkVarianceMovesToPublished()
    {
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("code-1", "title-662");
        try
        {
            item.MoveTo(StatementOfWorkVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkVarianceStatus.Published, "status");
    }

    public static void StatementOfWorkVarianceRejectsMovesFromRetired()
    {
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("code-1", "title-662");
        item.Status = StatementOfWorkVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(StatementOfWorkVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkVarianceStartsInCreated();
        StatementOfWorkVarianceMovesToPublished();
        StatementOfWorkVarianceRejectsMovesFromRetired();
    }
}
