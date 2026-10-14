namespace Acme.Workforce.Workplace.Tests.Desk.Agreement;

using Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeskAgreementChecks
{
    public static void DeskAgreementStartsInRequested()
    {
        var item = DeskAgreement.NewDeskAgreement("code-858", "label-47");
        Check.Equal(item.Status, DeskAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskAgreementMovesToRevoked()
    {
        var item = DeskAgreement.NewDeskAgreement("code-858", "label-47");
        try
        {
            item.MoveTo(DeskAgreementStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskAgreementStatus.Revoked, "status");
    }

    public static void DeskAgreementRejectsMovesFromExpired()
    {
        var item = DeskAgreement.NewDeskAgreement("code-858", "label-47");
        item.Status = DeskAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(DeskAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DeskAgreementStartsInRequested();
        DeskAgreementMovesToRevoked();
        DeskAgreementRejectsMovesFromExpired();
    }
}
