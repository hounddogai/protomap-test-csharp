namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnAuditChecks
{
    public static void AssetReturnAuditStartsInProposed()
    {
        var item = AssetReturnAudit.NewAssetReturnAudit("code-97", "name-790");
        Check.Equal(item.Status, AssetReturnAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnAuditMovesToApproved()
    {
        var item = AssetReturnAudit.NewAssetReturnAudit("code-97", "name-790");
        try
        {
            item.MoveTo(AssetReturnAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnAuditStatus.Approved, "status");
    }

    public static void AssetReturnAuditRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnAudit.NewAssetReturnAudit("code-97", "name-790");
        item.Status = AssetReturnAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnAuditStartsInProposed();
        AssetReturnAuditMovesToApproved();
        AssetReturnAuditRejectsMovesFromWithdrawn();
    }
}
