namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AttestationRequestChecks
{
    public static void AttestationRequestStartsInCreated()
    {
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
        Check.Equal(item.Status, AttestationRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationRequestMovesToValidated()
    {
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
        try
        {
            item.MoveTo(AttestationRequestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationRequestStatus.Validated, "status");
    }

    public static void AttestationRequestRejectsMovesFromRetired()
    {
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
        item.Status = AttestationRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(AttestationRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        AttestationRequestStartsInCreated();
        AttestationRequestMovesToValidated();
        AttestationRequestRejectsMovesFromRetired();
    }
}
