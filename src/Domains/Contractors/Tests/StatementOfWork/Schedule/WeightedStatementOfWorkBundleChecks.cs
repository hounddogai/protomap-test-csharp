namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkBundleChecks
{
    public static void WeightedStatementOfWorkBundleStartsInCreated()
    {
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("code-309", "name-146");
        Check.Equal(item.Status, WeightedStatementOfWorkBundleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedStatementOfWorkBundleMovesToValidated()
    {
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("code-309", "name-146");
        try
        {
            item.MoveTo(WeightedStatementOfWorkBundleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedStatementOfWorkBundleStatus.Validated, "status");
    }

    public static void WeightedStatementOfWorkBundleRejectsMovesFromRetired()
    {
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("code-309", "name-146");
        item.Status = WeightedStatementOfWorkBundleStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedStatementOfWorkBundleStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkBundleStartsInCreated();
        WeightedStatementOfWorkBundleMovesToValidated();
        WeightedStatementOfWorkBundleRejectsMovesFromRetired();
    }
}
