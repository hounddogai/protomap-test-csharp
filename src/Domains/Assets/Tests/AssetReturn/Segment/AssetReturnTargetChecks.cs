namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AssetReturnTargetChecks
{
    public static void AssetReturnTargetStartsInOpen()
    {
        var item = AssetReturnTarget.NewAssetReturnTarget("code-619", "label-621");
        Check.Equal(item.Status, AssetReturnTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnTargetMovesToReopened()
    {
        var item = AssetReturnTarget.NewAssetReturnTarget("code-619", "label-621");
        try
        {
            item.MoveTo(AssetReturnTargetStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnTargetStatus.Reopened, "status");
    }

    public static void AssetReturnTargetRejectsMovesFromArchived()
    {
        var item = AssetReturnTarget.NewAssetReturnTarget("code-619", "label-621");
        item.Status = AssetReturnTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(AssetReturnTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssetReturnTargetStartsInOpen();
        AssetReturnTargetMovesToReopened();
        AssetReturnTargetRejectsMovesFromArchived();
    }
}
