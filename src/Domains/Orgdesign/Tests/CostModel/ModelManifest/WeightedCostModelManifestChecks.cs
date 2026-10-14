namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class WeightedCostModelManifestChecks
{
    public static void WeightedCostModelManifestStartsInProposed()
    {
        var item = WeightedCostModelManifest.NewWeightedCostModelManifest("code-284", "name-702");
        Check.Equal(item.Status, WeightedCostModelManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedCostModelManifestMovesToWithdrawn()
    {
        var item = WeightedCostModelManifest.NewWeightedCostModelManifest("code-284", "name-702");
        try
        {
            item.MoveTo(WeightedCostModelManifestStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedCostModelManifestStatus.Withdrawn, "status");
    }

    public static void WeightedCostModelManifestRejectsMovesFromWithdrawn()
    {
        var item = WeightedCostModelManifest.NewWeightedCostModelManifest("code-284", "name-702");
        item.Status = WeightedCostModelManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedCostModelManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedCostModelManifestStartsInProposed();
        WeightedCostModelManifestMovesToWithdrawn();
        WeightedCostModelManifestRejectsMovesFromWithdrawn();
    }
}
