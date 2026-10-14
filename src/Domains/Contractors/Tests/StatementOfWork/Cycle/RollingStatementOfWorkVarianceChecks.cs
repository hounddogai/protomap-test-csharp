namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingStatementOfWorkVarianceChecks
{
    public static void RollingStatementOfWorkVarianceStartsInCreated()
    {
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("code-565", "title-301");
        Check.Equal(item.Status, RollingStatementOfWorkVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingStatementOfWorkVarianceMovesToRetired()
    {
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("code-565", "title-301");
        try
        {
            item.MoveTo(RollingStatementOfWorkVarianceStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingStatementOfWorkVarianceStatus.Retired, "status");
    }

    public static void RollingStatementOfWorkVarianceRejectsMovesFromRetired()
    {
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("code-565", "title-301");
        item.Status = RollingStatementOfWorkVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingStatementOfWorkVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingStatementOfWorkVarianceStartsInCreated();
        RollingStatementOfWorkVarianceMovesToRetired();
        RollingStatementOfWorkVarianceRejectsMovesFromRetired();
    }
}
