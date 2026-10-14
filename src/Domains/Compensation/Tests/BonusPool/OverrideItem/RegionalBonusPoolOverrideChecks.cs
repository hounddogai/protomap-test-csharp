namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalBonusPoolOverrideChecks
{
    public static void RegionalBonusPoolOverrideStartsInNew()
    {
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
        Check.Equal(item.Status, RegionalBonusPoolOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBonusPoolOverrideMovesToQueued()
    {
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
        try
        {
            item.MoveTo(RegionalBonusPoolOverrideStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBonusPoolOverrideStatus.Queued, "status");
    }

    public static void RegionalBonusPoolOverrideRejectsMovesFromFailed()
    {
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
        item.Status = RegionalBonusPoolOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalBonusPoolOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalBonusPoolOverrideStartsInNew();
        RegionalBonusPoolOverrideMovesToQueued();
        RegionalBonusPoolOverrideRejectsMovesFromFailed();
    }
}
