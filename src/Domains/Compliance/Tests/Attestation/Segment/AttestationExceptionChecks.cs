namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AttestationExceptionChecks
{
    public static void AttestationExceptionStartsInPlanned()
    {
        var item = AttestationException.NewAttestationException("code-467", "title-812");
        Check.Equal(item.Status, AttestationExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationExceptionMovesToScheduled()
    {
        var item = AttestationException.NewAttestationException("code-467", "title-812");
        try
        {
            item.MoveTo(AttestationExceptionStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationExceptionStatus.Scheduled, "status");
    }

    public static void AttestationExceptionRejectsMovesFromCancelled()
    {
        var item = AttestationException.NewAttestationException("code-467", "title-812");
        item.Status = AttestationExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AttestationExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AttestationExceptionStartsInPlanned();
        AttestationExceptionMovesToScheduled();
        AttestationExceptionRejectsMovesFromCancelled();
    }
}
