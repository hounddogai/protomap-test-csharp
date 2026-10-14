namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnPreferenceChecks
{
    public static void AssetReturnPreferenceStartsInNew()
    {
        var item = AssetReturnPreference.NewAssetReturnPreference("code-672", "label-276");
        Check.Equal(item.Status, AssetReturnPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnPreferenceMovesToQueued()
    {
        var item = AssetReturnPreference.NewAssetReturnPreference("code-672", "label-276");
        try
        {
            item.MoveTo(AssetReturnPreferenceStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnPreferenceStatus.Queued, "status");
    }

    public static void AssetReturnPreferenceRejectsMovesFromFailed()
    {
        var item = AssetReturnPreference.NewAssetReturnPreference("code-672", "label-276");
        item.Status = AssetReturnPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(AssetReturnPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        AssetReturnPreferenceStartsInNew();
        AssetReturnPreferenceMovesToQueued();
        AssetReturnPreferenceRejectsMovesFromFailed();
    }
}
