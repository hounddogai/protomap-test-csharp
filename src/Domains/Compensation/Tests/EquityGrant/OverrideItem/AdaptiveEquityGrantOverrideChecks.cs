namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AdaptiveEquityGrantOverrideChecks
{
    public static void AdaptiveEquityGrantOverrideStartsInNew()
    {
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("code-679", "name-28");
        Check.Equal(item.Status, AdaptiveEquityGrantOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveEquityGrantOverrideMovesToDone()
    {
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("code-679", "name-28");
        try
        {
            item.MoveTo(AdaptiveEquityGrantOverrideStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveEquityGrantOverrideStatus.Done, "status");
    }

    public static void AdaptiveEquityGrantOverrideRejectsMovesFromFailed()
    {
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("code-679", "name-28");
        item.Status = AdaptiveEquityGrantOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveEquityGrantOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveEquityGrantOverrideStartsInNew();
        AdaptiveEquityGrantOverrideMovesToDone();
        AdaptiveEquityGrantOverrideRejectsMovesFromFailed();
    }
}
