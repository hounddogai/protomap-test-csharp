namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnWindowChecks
{
    public static void AssetReturnWindowStartsInPlanned()
    {
        var item = AssetReturnWindow.NewAssetReturnWindow("code-673", "title-120");
        Check.Equal(item.Status, AssetReturnWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnWindowMovesToCompleted()
    {
        var item = AssetReturnWindow.NewAssetReturnWindow("code-673", "title-120");
        try
        {
            item.MoveTo(AssetReturnWindowStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnWindowStatus.Completed, "status");
    }

    public static void AssetReturnWindowRejectsMovesFromCancelled()
    {
        var item = AssetReturnWindow.NewAssetReturnWindow("code-673", "title-120");
        item.Status = AssetReturnWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssetReturnWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssetReturnWindowStartsInPlanned();
        AssetReturnWindowMovesToCompleted();
        AssetReturnWindowRejectsMovesFromCancelled();
    }
}
