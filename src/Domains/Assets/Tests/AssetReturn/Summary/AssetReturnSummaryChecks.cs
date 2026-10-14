namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnSummaryChecks
{
    public static void AssetReturnSummaryStartsInProposed()
    {
        var item = AssetReturnSummary.NewAssetReturnSummary("code-98", "label-790");
        Check.Equal(item.Status, AssetReturnSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnSummaryMovesToApproved()
    {
        var item = AssetReturnSummary.NewAssetReturnSummary("code-98", "label-790");
        try
        {
            item.MoveTo(AssetReturnSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnSummaryStatus.Approved, "status");
    }

    public static void AssetReturnSummaryRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnSummary.NewAssetReturnSummary("code-98", "label-790");
        item.Status = AssetReturnSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnSummaryStartsInProposed();
        AssetReturnSummaryMovesToApproved();
        AssetReturnSummaryRejectsMovesFromWithdrawn();
    }
}
