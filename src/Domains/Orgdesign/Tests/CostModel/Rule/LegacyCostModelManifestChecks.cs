namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyCostModelManifestChecks
{
    public static void LegacyCostModelManifestStartsInProposed()
    {
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
        Check.Equal(item.Status, LegacyCostModelManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyCostModelManifestMovesToWithdrawn()
    {
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
        try
        {
            item.MoveTo(LegacyCostModelManifestStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyCostModelManifestStatus.Withdrawn, "status");
    }

    public static void LegacyCostModelManifestRejectsMovesFromWithdrawn()
    {
        var item = LegacyCostModelManifest.NewLegacyCostModelManifest("code-910", "title-805");
        item.Status = LegacyCostModelManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyCostModelManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyCostModelManifestStartsInProposed();
        LegacyCostModelManifestMovesToWithdrawn();
        LegacyCostModelManifestRejectsMovesFromWithdrawn();
    }
}
