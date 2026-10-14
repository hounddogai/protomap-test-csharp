namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAgreementChecks
{
    public static void TieredStatementOfWorkAgreementStartsInOpen()
    {
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
        Check.Equal(item.Status, TieredStatementOfWorkAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredStatementOfWorkAgreementMovesToReopened()
    {
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
        try
        {
            item.MoveTo(TieredStatementOfWorkAgreementStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredStatementOfWorkAgreementStatus.Reopened, "status");
    }

    public static void TieredStatementOfWorkAgreementRejectsMovesFromArchived()
    {
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
        item.Status = TieredStatementOfWorkAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredStatementOfWorkAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAgreementStartsInOpen();
        TieredStatementOfWorkAgreementMovesToReopened();
        TieredStatementOfWorkAgreementRejectsMovesFromArchived();
    }
}
