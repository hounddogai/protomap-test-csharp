namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnPlanChecks
{
    public static void AssetReturnPlanStartsInCreated()
    {
        var item = AssetReturnPlan.NewAssetReturnPlan("code-852", "title-255");
        Check.Equal(item.Status, AssetReturnPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnPlanMovesToPublished()
    {
        var item = AssetReturnPlan.NewAssetReturnPlan("code-852", "title-255");
        try
        {
            item.MoveTo(AssetReturnPlanStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnPlanStatus.Published, "status");
    }

    public static void AssetReturnPlanRejectsMovesFromRetired()
    {
        var item = AssetReturnPlan.NewAssetReturnPlan("code-852", "title-255");
        item.Status = AssetReturnPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(AssetReturnPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        AssetReturnPlanStartsInCreated();
        AssetReturnPlanMovesToPublished();
        AssetReturnPlanRejectsMovesFromRetired();
    }
}
