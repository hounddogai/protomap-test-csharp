namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AttestationProfileChecks
{
    public static void AttestationProfileStartsInNew()
    {
        var item = AttestationProfile.NewAttestationProfile("code-296", "label-743");
        Check.Equal(item.Status, AttestationProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationProfileMovesToQueued()
    {
        var item = AttestationProfile.NewAttestationProfile("code-296", "label-743");
        try
        {
            item.MoveTo(AttestationProfileStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationProfileStatus.Queued, "status");
    }

    public static void AttestationProfileRejectsMovesFromFailed()
    {
        var item = AttestationProfile.NewAttestationProfile("code-296", "label-743");
        item.Status = AttestationProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(AttestationProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        AttestationProfileStartsInNew();
        AttestationProfileMovesToQueued();
        AttestationProfileRejectsMovesFromFailed();
    }
}
