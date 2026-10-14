namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class SeasonalPayBandCycleChecks
{
    public static void SeasonalPayBandCycleStartsInProposed()
    {
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("code-524", "label-3");
        Check.Equal(item.Status, SeasonalPayBandCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayBandCycleMovesToApproved()
    {
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("code-524", "label-3");
        try
        {
            item.MoveTo(SeasonalPayBandCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayBandCycleStatus.Approved, "status");
    }

    public static void SeasonalPayBandCycleRejectsMovesFromWithdrawn()
    {
        var item = SeasonalPayBandCycle.NewSeasonalPayBandCycle("code-524", "label-3");
        item.Status = SeasonalPayBandCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalPayBandCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalPayBandCycleStartsInProposed();
        SeasonalPayBandCycleMovesToApproved();
        SeasonalPayBandCycleRejectsMovesFromWithdrawn();
    }
}
