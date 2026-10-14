namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientAssetReturnTargetChecks
{
    public static void LenientAssetReturnTargetStartsInRequested()
    {
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("code-566", "name-521");
        Check.Equal(item.Status, LenientAssetReturnTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAssetReturnTargetMovesToGranted()
    {
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("code-566", "name-521");
        try
        {
            item.MoveTo(LenientAssetReturnTargetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAssetReturnTargetStatus.Granted, "status");
    }

    public static void LenientAssetReturnTargetRejectsMovesFromExpired()
    {
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("code-566", "name-521");
        item.Status = LenientAssetReturnTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientAssetReturnTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientAssetReturnTargetStartsInRequested();
        LenientAssetReturnTargetMovesToGranted();
        LenientAssetReturnTargetRejectsMovesFromExpired();
    }
}
