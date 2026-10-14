namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestChecks
{
    public static void RegionalStatementOfWorkDigestStartsInNew()
    {
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
        Check.Equal(item.Status, RegionalStatementOfWorkDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalStatementOfWorkDigestMovesToDone()
    {
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
        try
        {
            item.MoveTo(RegionalStatementOfWorkDigestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalStatementOfWorkDigestStatus.Done, "status");
    }

    public static void RegionalStatementOfWorkDigestRejectsMovesFromFailed()
    {
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
        item.Status = RegionalStatementOfWorkDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalStatementOfWorkDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkDigestStartsInNew();
        RegionalStatementOfWorkDigestMovesToDone();
        RegionalStatementOfWorkDigestRejectsMovesFromFailed();
    }
}
