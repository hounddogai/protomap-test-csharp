namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class ManualAttestationEstimateChecks
{
    public static void ManualAttestationEstimateStartsInPlanned()
    {
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("code-142", "label-981");
        Check.Equal(item.Status, ManualAttestationEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAttestationEstimateMovesToInProgress()
    {
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("code-142", "label-981");
        try
        {
            item.MoveTo(ManualAttestationEstimateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAttestationEstimateStatus.InProgress, "status");
    }

    public static void ManualAttestationEstimateRejectsMovesFromCancelled()
    {
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("code-142", "label-981");
        item.Status = ManualAttestationEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualAttestationEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualAttestationEstimateStartsInPlanned();
        ManualAttestationEstimateMovesToInProgress();
        ManualAttestationEstimateRejectsMovesFromCancelled();
    }
}
