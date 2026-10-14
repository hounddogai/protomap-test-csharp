namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class NominationAgreementChecks
{
    public static void NominationAgreementStartsInProposed()
    {
        var item = NominationAgreement.NewNominationAgreement("code-412", "title-100");
        Check.Equal(item.Status, NominationAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationAgreementMovesToRejected()
    {
        var item = NominationAgreement.NewNominationAgreement("code-412", "title-100");
        try
        {
            item.MoveTo(NominationAgreementStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationAgreementStatus.Rejected, "status");
    }

    public static void NominationAgreementRejectsMovesFromWithdrawn()
    {
        var item = NominationAgreement.NewNominationAgreement("code-412", "title-100");
        item.Status = NominationAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(NominationAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        NominationAgreementStartsInProposed();
        NominationAgreementMovesToRejected();
        NominationAgreementRejectsMovesFromWithdrawn();
    }
}
