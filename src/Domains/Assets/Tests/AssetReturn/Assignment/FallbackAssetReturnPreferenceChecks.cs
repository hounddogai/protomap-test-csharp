namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackAssetReturnPreferenceChecks
{
    public static void FallbackAssetReturnPreferenceStartsInCreated()
    {
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("code-421", "label-508");
        Check.Equal(item.Status, FallbackAssetReturnPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAssetReturnPreferenceMovesToPublished()
    {
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("code-421", "label-508");
        try
        {
            item.MoveTo(FallbackAssetReturnPreferenceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAssetReturnPreferenceStatus.Published, "status");
    }

    public static void FallbackAssetReturnPreferenceRejectsMovesFromRetired()
    {
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("code-421", "label-508");
        item.Status = FallbackAssetReturnPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackAssetReturnPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackAssetReturnPreferenceStartsInCreated();
        FallbackAssetReturnPreferenceMovesToPublished();
        FallbackAssetReturnPreferenceRejectsMovesFromRetired();
    }
}
