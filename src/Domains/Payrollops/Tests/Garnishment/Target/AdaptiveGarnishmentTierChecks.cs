namespace Acme.Workforce.Payrollops.Tests.Garnishment.Target;

using Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentTierChecks
{
    public static void AdaptiveGarnishmentTierStartsInDraft()
    {
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("code-833", "name-594");
        Check.Equal(item.Status, AdaptiveGarnishmentTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveGarnishmentTierMovesToSuspended()
    {
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("code-833", "name-594");
        try
        {
            item.MoveTo(AdaptiveGarnishmentTierStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveGarnishmentTierStatus.Suspended, "status");
    }

    public static void AdaptiveGarnishmentTierRejectsMovesFromClosed()
    {
        var item = AdaptiveGarnishmentTier.NewAdaptiveGarnishmentTier("code-833", "name-594");
        item.Status = AdaptiveGarnishmentTierStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveGarnishmentTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentTierStartsInDraft();
        AdaptiveGarnishmentTierMovesToSuspended();
        AdaptiveGarnishmentTierRejectsMovesFromClosed();
    }
}
