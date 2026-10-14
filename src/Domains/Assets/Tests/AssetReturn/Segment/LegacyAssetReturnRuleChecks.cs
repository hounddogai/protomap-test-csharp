namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyAssetReturnRuleChecks
{
    public static void LegacyAssetReturnRuleStartsInPlanned()
    {
        var item = LegacyAssetReturnRule.NewLegacyAssetReturnRule("code-354", "name-288");
        Check.Equal(item.Status, LegacyAssetReturnRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAssetReturnRuleMovesToCancelled()
    {
        var item = LegacyAssetReturnRule.NewLegacyAssetReturnRule("code-354", "name-288");
        try
        {
            item.MoveTo(LegacyAssetReturnRuleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAssetReturnRuleStatus.Cancelled, "status");
    }

    public static void LegacyAssetReturnRuleRejectsMovesFromCancelled()
    {
        var item = LegacyAssetReturnRule.NewLegacyAssetReturnRule("code-354", "name-288");
        item.Status = LegacyAssetReturnRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyAssetReturnRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyAssetReturnRuleStartsInPlanned();
        LegacyAssetReturnRuleMovesToCancelled();
        LegacyAssetReturnRuleRejectsMovesFromCancelled();
    }
}
