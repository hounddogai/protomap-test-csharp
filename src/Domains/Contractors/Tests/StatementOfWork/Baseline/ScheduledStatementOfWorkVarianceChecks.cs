namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkVarianceChecks
{
    public static void ScheduledStatementOfWorkVarianceStartsInCreated()
    {
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("code-868", "name-629");
        Check.Equal(item.Status, ScheduledStatementOfWorkVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledStatementOfWorkVarianceMovesToRetired()
    {
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("code-868", "name-629");
        try
        {
            item.MoveTo(ScheduledStatementOfWorkVarianceStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledStatementOfWorkVarianceStatus.Retired, "status");
    }

    public static void ScheduledStatementOfWorkVarianceRejectsMovesFromRetired()
    {
        var item = ScheduledStatementOfWorkVariance.NewScheduledStatementOfWorkVariance("code-868", "name-629");
        item.Status = ScheduledStatementOfWorkVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledStatementOfWorkVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkVarianceStartsInCreated();
        ScheduledStatementOfWorkVarianceMovesToRetired();
        ScheduledStatementOfWorkVarianceRejectsMovesFromRetired();
    }
}
