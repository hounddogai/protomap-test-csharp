namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LegacyAssetReturnPlanChecks
{
    public static void LegacyAssetReturnPlanStartsInOpen()
    {
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("code-983", "title-160");
        Check.Equal(item.Status, LegacyAssetReturnPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAssetReturnPlanMovesToInReview()
    {
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("code-983", "title-160");
        try
        {
            item.MoveTo(LegacyAssetReturnPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAssetReturnPlanStatus.InReview, "status");
    }

    public static void LegacyAssetReturnPlanRejectsMovesFromArchived()
    {
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("code-983", "title-160");
        item.Status = LegacyAssetReturnPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyAssetReturnPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyAssetReturnPlanStartsInOpen();
        LegacyAssetReturnPlanMovesToInReview();
        LegacyAssetReturnPlanRejectsMovesFromArchived();
    }
}
