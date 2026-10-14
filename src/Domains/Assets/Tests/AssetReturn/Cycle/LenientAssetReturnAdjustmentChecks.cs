namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientAssetReturnAdjustmentChecks
{
    public static void LenientAssetReturnAdjustmentStartsInRequested()
    {
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("code-268", "title-245");
        Check.Equal(item.Status, LenientAssetReturnAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAssetReturnAdjustmentMovesToGranted()
    {
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("code-268", "title-245");
        try
        {
            item.MoveTo(LenientAssetReturnAdjustmentStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAssetReturnAdjustmentStatus.Granted, "status");
    }

    public static void LenientAssetReturnAdjustmentRejectsMovesFromExpired()
    {
        var item = LenientAssetReturnAdjustment.NewLenientAssetReturnAdjustment("code-268", "title-245");
        item.Status = LenientAssetReturnAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientAssetReturnAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientAssetReturnAdjustmentStartsInRequested();
        LenientAssetReturnAdjustmentMovesToGranted();
        LenientAssetReturnAdjustmentRejectsMovesFromExpired();
    }
}
