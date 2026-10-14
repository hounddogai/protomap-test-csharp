namespace Acme.Workforce.Contractors.Tests.Offboarding.Agreement;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class OffboardingAgreementChecks
{
    public static void OffboardingAgreementStartsInOpen()
    {
        var item = OffboardingAgreement.NewOffboardingAgreement("code-819", "label-714");
        Check.Equal(item.Status, OffboardingAgreementStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingAgreementMovesToResolved()
    {
        var item = OffboardingAgreement.NewOffboardingAgreement("code-819", "label-714");
        try
        {
            item.MoveTo(OffboardingAgreementStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingAgreementStatus.Resolved, "status");
    }

    public static void OffboardingAgreementRejectsMovesFromArchived()
    {
        var item = OffboardingAgreement.NewOffboardingAgreement("code-819", "label-714");
        item.Status = OffboardingAgreementStatus.Archived;
        Check.Fails(() => item.MoveTo(OffboardingAgreementStatus.Open), "move");
    }

    public static void RunAll()
    {
        OffboardingAgreementStartsInOpen();
        OffboardingAgreementMovesToResolved();
        OffboardingAgreementRejectsMovesFromArchived();
    }
}
