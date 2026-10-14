namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualAgreementChecks
{
    public static void AccrualAgreementStartsInOpen()
    {
        var item = AccrualAgreement.NewAccrualAgreement("code-969", "title-95");
        Check.Equal(item.Status, AccrualAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualAgreementMovesToInReview()
    {
        var item = AccrualAgreement.NewAccrualAgreement("code-969", "title-95");
        try
        {
            item.MoveTo(AccrualAgreementStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualAgreementStatus.InReview, "status");
    }

    public static void AccrualAgreementRejectsMovesFromArchived()
    {
        var item = AccrualAgreement.NewAccrualAgreement("code-969", "title-95");
        item.Status = AccrualAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(AccrualAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccrualAgreementStartsInOpen();
        AccrualAgreementMovesToInReview();
        AccrualAgreementRejectsMovesFromArchived();
    }
}
