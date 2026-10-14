namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationEstimateChecks
{
    public static void StrictAttestationEstimateStartsInRequested()
    {
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("code-219", "name-582");
        Check.Equal(item.Status, StrictAttestationEstimateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAttestationEstimateMovesToExpired()
    {
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("code-219", "name-582");
        try
        {
            item.MoveTo(StrictAttestationEstimateStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAttestationEstimateStatus.Expired, "status");
    }

    public static void StrictAttestationEstimateRejectsMovesFromExpired()
    {
        var item = StrictAttestationEstimate.NewStrictAttestationEstimate("code-219", "name-582");
        item.Status = StrictAttestationEstimateStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictAttestationEstimateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictAttestationEstimateStartsInRequested();
        StrictAttestationEstimateMovesToExpired();
        StrictAttestationEstimateRejectsMovesFromExpired();
    }
}
