namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestChecks
{
    public static void AdjustmentManifestStartsInProposed()
    {
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
        Check.Equal(item.Status, AdjustmentManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentManifestMovesToApproved()
    {
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
        try
        {
            item.MoveTo(AdjustmentManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentManifestStatus.Approved, "status");
    }

    public static void AdjustmentManifestRejectsMovesFromWithdrawn()
    {
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
        item.Status = AdjustmentManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdjustmentManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdjustmentManifestStartsInProposed();
        AdjustmentManifestMovesToApproved();
        AdjustmentManifestRejectsMovesFromWithdrawn();
    }
}
