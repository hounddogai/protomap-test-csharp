namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupChecks
{
    public static void AssetReturnGroupStartsInNew()
    {
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
        Check.Equal(item.Status, AssetReturnGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnGroupMovesToDone()
    {
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
        try
        {
            item.MoveTo(AssetReturnGroupStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnGroupStatus.Done, "status");
    }

    public static void AssetReturnGroupRejectsMovesFromFailed()
    {
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
        item.Status = AssetReturnGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnGroupStartsInNew();
        AssetReturnGroupMovesToDone();
        AssetReturnGroupRejectsMovesFromFailed();
    }
}
