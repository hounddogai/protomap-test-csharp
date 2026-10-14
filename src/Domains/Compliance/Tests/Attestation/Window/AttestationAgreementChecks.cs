namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationAgreementChecks
{
    public static void AttestationAgreementStartsInNew()
    {
        var item = AttestationAgreement.NewAttestationAgreement("code-417", "label-6");
        Check.Equal(item.Status, AttestationAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationAgreementMovesToQueued()
    {
        var item = AttestationAgreement.NewAttestationAgreement("code-417", "label-6");
        try
        {
            item.MoveTo(AttestationAgreementStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationAgreementStatus.Queued, "status");
    }

    public static void AttestationAgreementRejectsMovesFromFailed()
    {
        var item = AttestationAgreement.NewAttestationAgreement("code-417", "label-6");
        item.Status = AttestationAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(AttestationAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        AttestationAgreementStartsInNew();
        AttestationAgreementMovesToQueued();
        AttestationAgreementRejectsMovesFromFailed();
    }
}
