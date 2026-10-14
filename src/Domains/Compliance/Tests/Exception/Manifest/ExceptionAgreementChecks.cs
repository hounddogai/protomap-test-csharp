namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAgreementChecks
{
    public static void ExceptionAgreementStartsInProposed()
    {
        var item = ExceptionAgreement.NewExceptionAgreement("code-28", "title-808");
        Check.Equal(item.Status, ExceptionAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionAgreementMovesToApproved()
    {
        var item = ExceptionAgreement.NewExceptionAgreement("code-28", "title-808");
        try
        {
            item.MoveTo(ExceptionAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionAgreementStatus.Approved, "status");
    }

    public static void ExceptionAgreementRejectsMovesFromWithdrawn()
    {
        var item = ExceptionAgreement.NewExceptionAgreement("code-28", "title-808");
        item.Status = ExceptionAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ExceptionAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ExceptionAgreementStartsInProposed();
        ExceptionAgreementMovesToApproved();
        ExceptionAgreementRejectsMovesFromWithdrawn();
    }
}
