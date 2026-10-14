namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AssetReturnPolicyChecks
{
    public static void AssetReturnPolicyStartsInOpen()
    {
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
        Check.Equal(item.Status, AssetReturnPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnPolicyMovesToResolved()
    {
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
        try
        {
            item.MoveTo(AssetReturnPolicyStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnPolicyStatus.Resolved, "status");
    }

    public static void AssetReturnPolicyRejectsMovesFromArchived()
    {
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
        item.Status = AssetReturnPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnPolicyStartsInOpen();
        AssetReturnPolicyMovesToResolved();
        AssetReturnPolicyRejectsMovesFromArchived();
    }
}
