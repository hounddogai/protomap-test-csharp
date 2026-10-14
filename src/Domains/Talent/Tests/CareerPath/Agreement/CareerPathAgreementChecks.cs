namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathAgreementChecks
{
    public static void CareerPathAgreementStartsInDraft()
    {
        var item = CareerPathAgreement.NewCareerPathAgreement("code-552", "name-118");
        Check.Equal(item.Status, CareerPathAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathAgreementMovesToPending()
    {
        var item = CareerPathAgreement.NewCareerPathAgreement("code-552", "name-118");
        try
        {
            item.MoveTo(CareerPathAgreementStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathAgreementStatus.Pending, "status");
    }

    public static void CareerPathAgreementRejectsMovesFromClosed()
    {
        var item = CareerPathAgreement.NewCareerPathAgreement("code-552", "name-118");
        item.Status = CareerPathAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(CareerPathAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CareerPathAgreementStartsInDraft();
        CareerPathAgreementMovesToPending();
        CareerPathAgreementRejectsMovesFromClosed();
    }
}
