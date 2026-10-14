namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalTownHallClaimEngineChecks
{
    public static void SeasonalTownHallClaimEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalTownHallClaimEngine.DefaultSeasonalTownHallClaimEngine(Flags.NewRegistry());
        var item = SeasonalTownHallClaim.NewSeasonalTownHallClaim("code-574", "label-903");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalTownHallClaimEngineKeepsResultsWithinBounds();
    }
}
