namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkAgreementChecks
{
    public static void LegacyStatementOfWorkAgreementStartsInCreated()
    {
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("code-846", "title-86");
        Check.Equal(item.Status, LegacyStatementOfWorkAgreementStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyStatementOfWorkAgreementMovesToValidated()
    {
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("code-846", "title-86");
        try
        {
            item.MoveTo(LegacyStatementOfWorkAgreementStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyStatementOfWorkAgreementStatus.Validated, "status");
    }

    public static void LegacyStatementOfWorkAgreementRejectsMovesFromRetired()
    {
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("code-846", "title-86");
        item.Status = LegacyStatementOfWorkAgreementStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyStatementOfWorkAgreementStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkAgreementStartsInCreated();
        LegacyStatementOfWorkAgreementMovesToValidated();
        LegacyStatementOfWorkAgreementRejectsMovesFromRetired();
    }
}
