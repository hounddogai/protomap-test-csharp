namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class FallbackAssetReturnDraftChecks
{
    public static void FallbackAssetReturnDraftStartsInNew()
    {
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("code-492", "title-409");
        Check.Equal(item.Status, FallbackAssetReturnDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAssetReturnDraftMovesToRunning()
    {
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("code-492", "title-409");
        try
        {
            item.MoveTo(FallbackAssetReturnDraftStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAssetReturnDraftStatus.Running, "status");
    }

    public static void FallbackAssetReturnDraftRejectsMovesFromFailed()
    {
        var item = FallbackAssetReturnDraft.NewFallbackAssetReturnDraft("code-492", "title-409");
        item.Status = FallbackAssetReturnDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackAssetReturnDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackAssetReturnDraftStartsInNew();
        FallbackAssetReturnDraftMovesToRunning();
        FallbackAssetReturnDraftRejectsMovesFromFailed();
    }
}
