namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingPositionManifestChecks
{
    public static void RollingPositionManifestStartsInProposed()
    {
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
        Check.Equal(item.Status, RollingPositionManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPositionManifestMovesToApproved()
    {
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
        try
        {
            item.MoveTo(RollingPositionManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPositionManifestStatus.Approved, "status");
    }

    public static void RollingPositionManifestRejectsMovesFromWithdrawn()
    {
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
        item.Status = RollingPositionManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingPositionManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingPositionManifestStartsInProposed();
        RollingPositionManifestMovesToApproved();
        RollingPositionManifestRejectsMovesFromWithdrawn();
    }
}
