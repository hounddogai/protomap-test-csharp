namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelManifestChecks
{
    public static void CostModelManifestStartsInCreated()
    {
        var item = CostModelManifest.NewCostModelManifest("code-416", "name-464");
        Check.Equal(item.Status, CostModelManifestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelManifestMovesToPublished()
    {
        var item = CostModelManifest.NewCostModelManifest("code-416", "name-464");
        try
        {
            item.MoveTo(CostModelManifestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelManifestStatus.Published, "status");
    }

    public static void CostModelManifestRejectsMovesFromRetired()
    {
        var item = CostModelManifest.NewCostModelManifest("code-416", "name-464");
        item.Status = CostModelManifestStatus.Retired;
        Check.Fails(() => item.MoveTo(CostModelManifestStatus.Created), "move");
    }

    public static void RunAll()
    {
        CostModelManifestStartsInCreated();
        CostModelManifestMovesToPublished();
        CostModelManifestRejectsMovesFromRetired();
    }
}
