namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class FallbackAttestationSummaryChecks
{
    public static void FallbackAttestationSummaryStartsInRequested()
    {
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("code-742", "name-486");
        Check.Equal(item.Status, FallbackAttestationSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAttestationSummaryMovesToGranted()
    {
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("code-742", "name-486");
        try
        {
            item.MoveTo(FallbackAttestationSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAttestationSummaryStatus.Granted, "status");
    }

    public static void FallbackAttestationSummaryRejectsMovesFromExpired()
    {
        var item = FallbackAttestationSummary.NewFallbackAttestationSummary("code-742", "name-486");
        item.Status = FallbackAttestationSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackAttestationSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackAttestationSummaryStartsInRequested();
        FallbackAttestationSummaryMovesToGranted();
        FallbackAttestationSummaryRejectsMovesFromExpired();
    }
}
