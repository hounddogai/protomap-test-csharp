namespace Acme.Workforce.Compliance.Tests.Attestation.Policy;

using Acme.Workforce.Compliance.Attestation.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class AttestationPreferenceChecks
{
    public static void AttestationPreferenceStartsInOpen()
    {
        var item = AttestationPreference.NewAttestationPreference("code-25", "label-548");
        Check.Equal(item.Status, AttestationPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationPreferenceMovesToInReview()
    {
        var item = AttestationPreference.NewAttestationPreference("code-25", "label-548");
        try
        {
            item.MoveTo(AttestationPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationPreferenceStatus.InReview, "status");
    }

    public static void AttestationPreferenceRejectsMovesFromArchived()
    {
        var item = AttestationPreference.NewAttestationPreference("code-25", "label-548");
        item.Status = AttestationPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(AttestationPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        AttestationPreferenceStartsInOpen();
        AttestationPreferenceMovesToInReview();
        AttestationPreferenceRejectsMovesFromArchived();
    }
}
