namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class AdaptivePayBandGroupChecks
{
    public static void AdaptivePayBandGroupStartsInDraft()
    {
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("code-876", "title-841");
        Check.Equal(item.Status, AdaptivePayBandGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePayBandGroupMovesToSuspended()
    {
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("code-876", "title-841");
        try
        {
            item.MoveTo(AdaptivePayBandGroupStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePayBandGroupStatus.Suspended, "status");
    }

    public static void AdaptivePayBandGroupRejectsMovesFromClosed()
    {
        var item = AdaptivePayBandGroup.NewAdaptivePayBandGroup("code-876", "title-841");
        item.Status = AdaptivePayBandGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptivePayBandGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptivePayBandGroupStartsInDraft();
        AdaptivePayBandGroupMovesToSuspended();
        AdaptivePayBandGroupRejectsMovesFromClosed();
    }
}
