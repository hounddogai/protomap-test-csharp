namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssetReturnRuleChecks
{
    public static void AssetReturnRuleStartsInIdle()
    {
        var item = AssetReturnRule.NewAssetReturnRule("code-965", "label-501");
        Check.Equal(item.Status, AssetReturnRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnRuleMovesToDraining()
    {
        var item = AssetReturnRule.NewAssetReturnRule("code-965", "label-501");
        try
        {
            item.MoveTo(AssetReturnRuleStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnRuleStatus.Draining, "status");
    }

    public static void AssetReturnRuleRejectsMovesFromStopped()
    {
        var item = AssetReturnRule.NewAssetReturnRule("code-965", "label-501");
        item.Status = AssetReturnRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssetReturnRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssetReturnRuleStartsInIdle();
        AssetReturnRuleMovesToDraining();
        AssetReturnRuleRejectsMovesFromStopped();
    }
}
